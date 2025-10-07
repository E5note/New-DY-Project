/// <summary>
/// 事件名， 尽量不要在代码内直接使用字符串，要不然查找哪里使用过会稍微麻烦一些，统一修改的时候也会麻烦一些
/// </summary>
public class EventName
{
    public static string InitGame = "初始化游戏";
    public static string AssetLoadProgress = "资源加载进度";
    public static string LanguageChanged = "切换语言";
    public static string EnterScene = "进入场景";
    public static string ExitScene = "退出场景";
    public static string WaveCompleted = "一波结束";
    public static string NextWave = "下一波";
    public static string NextAction = "下一条演出";

    public static string Clear = "清理";
    public static string Log = "日志";
    public static string Tags = "标签";
    public static string Move = "移动";
    public static string Die = "死亡";
    public static string ItemChange = "物品变动";

    public static string RandomAttackEnemy="对随机一名敌人造成伤害";
    public static string TakeDamage = "扣除被攻击者血量";
    public static string Attack = "攻击";
    public static string AttackOther = "攻击他人";
    public static string BeAttacked = "被他人攻击";
    public static string Count = "计数";
    public static string Count1 = "次数";
    public static string Count2 = "可触发次数";
    public static string Always = "总是";
    public static string BattleStart = "敌袭开始时";
    public static string BattleFinish = "敌袭结束后";
    public static string PickConsumables = "拾取消耗品时";
    public static string PickMaterial = "拾取材料时";
    public static string PickBox = "拾取箱子时";
    public static string EnterStore = "进入商店时";
    public static string Dodge = "躲避敌人攻击时";
    public static string CritKillEnemy = "暴击杀死敌人时";
    public static string EnemyDie = "敌人死亡时";//注意要区分情况 战斗结束的销毁不触发   
    public static string PlayerHit = "受到攻击时";
    public static string PlayerRealHit = "主角受击扣血时";
    public static string AttackEnemy = "击中敌人时";

    #region 角色属性相关
    public static string Position = "持有者位置";

    public static string Material = "材料";
    public static string CurLevel = "当前等级";
    public static string CurExp = "当前经验";
    public static string CurHP = "当前生命值";
    public static string MaxHP = "最大生命值";

    public static string MaxHPUL = "最大生命值上限";
    public static string HPRegeneration = "生命再生";
    public static string LifeSteal = "生命窃取";
    public static string Damage = "伤害";
    public static string MeleeDamage = "近战伤害";

    public static string RangedDamage = "远程伤害";
    public static string ElementalDamage = "属性伤害";
    public static string AttackSpeed = "攻击速度";
    public static string CritChance = "暴击率";
    public static string Engineering = "工程学";

    public static string Range = "范围";
    public static string Armor = "护甲";
    public static string DodgeRate = "闪避";
    public static string Speed = "速度";
    public static string Luck = "幸运";

    public static string Harvesting = "收获";
    public static string ConsumableHeal = "使用消耗品恢复";
    public static string XPGain = "获得经验";
    public static string PickupRange = "拾取范围";
    public static string ItemsPrice = "道具价格";

    public static string ExplosionDamage = "爆炸伤害";
    public static string ExplosionSize = "爆炸范围";
    public static string Bounces = "投射物反弹";
    public static string Piercing = "投射物贯通";
    public static string PiercingDamage = "贯通伤害";

    public static string DamageAgainstBosses = "对头目和精英怪的伤害系数";
    public static string BurningSpeed = "燃烧速度";
    public static string BurningSpeedRate = "燃烧速度比率";
    public static string BurnOther = "燃烧蔓延至附近的一名敌人";
    public static string Knockback = "击退";

    public static string DoubleMaterialChance = "材料翻倍概率";
    public static string FreeRerolls = "商店免费刷新次数";
    public static string Trees = "树木";
    public static string TreesKill = "树木能被一击毙命";
    public static string Enemies = "敌人";

    public static string EnemySpeed = "敌人速度";
    public static string EnemyDamageRate = "敌人伤害";
    public static string SpecialEnemy = "特殊敌人";
    public static string HealNegation = "无法通过其他途径恢复生命值";
    public static string Hatred = "仇恨值";

    public static string SpeedCutMax30 = "击中敌人时能使其降低10速度最高30";
    public static string ImmediatelyEatMaterialChance = "立即吸收掉落的材料";
    public static string ItemsRecyclePrice = "回收道具价格系数";
    public static string InjuryAvoidanceTimes = "免疫伤害次数";
    public static string RandomNumber = "随机数";
    #endregion

}
