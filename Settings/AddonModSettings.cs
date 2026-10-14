namespace STS2_WineFox_Addons.Settings
{
    /// <summary>
    /// 扩展 mod 的可配置项，由 RitsuLib 的数据存储负责读写与落盘。
    /// </summary>
    public sealed class AddonModSettings
    {
        /// <summary>
        /// 开启后不再要求队伍里必须有酒狐，任何角色都能在战斗结算时掉落食物。
        /// 默认关闭，即沿用酒狐本体 mod 的规则。
        /// </summary>
        public bool FoodForEveryone { get; set; }
    }
}
