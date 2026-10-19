#!/usr/bin/env python3
"""List or extract files from a Godot .pck archive.

This intentionally supports only the unpacked game-pack variants used by the
reference mod. It handles PCK format v1 and v2, including 64-bit file offsets.
"""

from __future__ import annotations

import argparse
import struct
from dataclasses import dataclass
from pathlib import Path
from typing import BinaryIO


PCK_MAGIC = 0x43504447  # GDPC


@dataclass(frozen=True)
class Entry:
    path: str
    offset: int
    size: int


def _read_cstring(stream: BinaryIO, length: int) -> str:
    raw = stream.read(length)
    if len(raw) != length:
        raise ValueError("Unexpected end of file while reading an entry path")
    return raw.rstrip(b"\0").decode("utf-8", errors="replace")


def _align4(value: int) -> int:
    return (value + 3) & ~3


def read_pck(path: Path) -> tuple[int, int, list[Entry]]:
    with path.open("rb") as stream:
        magic = struct.unpack("<I", stream.read(4))[0]
        if magic != PCK_MAGIC:
            raise ValueError(f"{path} is not a Godot PCK (magic=0x{magic:08x})")

        format_version = struct.unpack("<I", stream.read(4))[0]
        stream.read(12)  # Engine version: major, minor, patch.

        if format_version == 1:
            file_base = 0
            stream.read(16 * 4)
            directory_offset = stream.tell()
        elif format_version == 2:
            stream.read(4)  # Pack flags.
            file_base = struct.unpack("<Q", stream.read(8))[0]
            stream.read(16 * 4)
            directory_offset = stream.tell()
        elif format_version == 3:
            stream.read(4)  # Pack flags.
            file_base = struct.unpack("<Q", stream.read(8))[0]
            directory_offset = struct.unpack("<Q", stream.read(8))[0]
            stream.read(16 * 4)
        else:
            raise ValueError(f"Unsupported PCK format version: {format_version}")

        if format_version == 3:
            stream.seek(directory_offset)
        file_count = struct.unpack("<I", stream.read(4))[0]
        entries: list[Entry] = []

        for _ in range(file_count):
            path_length = struct.unpack("<I", stream.read(4))[0]
            entry_path = _read_cstring(stream, path_length)
            stream.read(_align4(path_length) - path_length)
            offset = struct.unpack("<Q", stream.read(8))[0]
            size = struct.unpack("<Q", stream.read(8))[0]
            stream.read(16)  # MD5
            if format_version >= 2:
                stream.read(4)  # File flags.

            # Godot stores entry offsets relative to the pack's file base.
            entries.append(Entry(entry_path, offset, size))

    return format_version, file_base, entries


def _safe_relative_path(value: str) -> Path:
    normalized = value.removeprefix("res://").lstrip("/\\")
    result = Path(normalized)
    if result.is_absolute() or ".." in result.parts:
        raise ValueError(f"Refusing unsafe archive path: {value}")
    return result


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("pck", type=Path)
    parser.add_argument("--list", action="store_true", help="Print archive entries")
    parser.add_argument(
        "--extract",
        metavar="PREFIX",
        action="append",
        default=[],
        help="Extract entries whose path starts with PREFIX (repeatable)",
    )
    parser.add_argument("--output", type=Path, help="Extraction directory")
    args = parser.parse_args()

    format_version, file_base, entries = read_pck(args.pck)
    if args.list or not args.extract:
        for entry in entries:
            print(entry.path)
        print(
            f"# {len(entries)} entries, PCK v{format_version}, base={file_base}",
            flush=True,
        )

    if args.extract:
        if args.output is None:
            parser.error("--output is required with --extract")
        selected = [
            entry
            for entry in entries
            if any(entry.path.removeprefix("res://").startswith(prefix) for prefix in args.extract)
        ]
        args.output.mkdir(parents=True, exist_ok=True)
        with args.pck.open("rb") as stream:
            for entry in selected:
                destination = args.output / _safe_relative_path(entry.path)
                destination.parent.mkdir(parents=True, exist_ok=True)
                stream.seek(file_base + entry.offset)
                data = stream.read(entry.size)
                if len(data) != entry.size:
                    raise ValueError(f"Truncated archive entry: {entry.path}")
                destination.write_bytes(data)
        print(f"# extracted {len(selected)} entries to {args.output}", flush=True)


if __name__ == "__main__":
    main()
