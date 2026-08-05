namespace WeaponSmith.Util;

public class EntryCategorizer
{
    public static EntryCategory Categorize(string key)
    {
        // First look up in dictionaries
        if (General.Contains(key))
            return EntryCategory.General;
        if (UI.Contains(key))
            return EntryCategory.UI;
        if (Anim.Contains(key))
            return EntryCategory.Anim;

        // If not set, then generalize
        if (key.Contains("damage", StringComparison.CurrentCultureIgnoreCase) || isDamageLoc(key))
            return EntryCategory.Damage;
        if (key.Contains("model", StringComparison.CurrentCultureIgnoreCase))
            return EntryCategory.Model;
        if (key.EndsWith("Anim", StringComparison.CurrentCultureIgnoreCase))
            return EntryCategory.Anim;
        if (key.Contains("sound", StringComparison.CurrentCultureIgnoreCase))
            return EntryCategory.Sound;
        if (key.Contains("effect", StringComparison.CurrentCultureIgnoreCase))
            return EntryCategory.FX;
        
        return EntryCategory.Misc;
    }

    public static bool isDamageLoc(string key)
    {
        return key.StartsWith("loc", StringComparison.CurrentCultureIgnoreCase) &&
               !key.StartsWith("lock", StringComparison.CurrentCultureIgnoreCase);
    }

    public static List<string> General = new List<string>()
    {
        "displayName",
        "weaponType",
        "weaponClass",
        "penetrateType",
        "impactType",
        "inventoryType",
        "fireType",
        "clipType",
        "barrelType",
        "offhandClass",
        "offhandSlot",
        "attachments",
        "attachmentUniques",
        "hideTags",
    };
    
    public static List<string> Anim = new List<string>()
    {
        "dtp_in",
        "dtp_loop",
        "dtp_out",
        "dtp_empty_in",
        "dtp_empty_loop",
        "dtp_empty_out",
        "slide_in"
    };
    
    public static List<string> UI = new List<string>()
    {
        "ammoCounterIcon",
        "hudIcon",
        "killIcon",
        "fireTypeIcon",
        "killIconRatio",
        "hudIconRatio",
        "dpadIconRatio",
        "ammoCounterIconRatio",
        "indicatorIconRatio",
        "noAmmoOnDpadIcon",
        "flipKillIcon",
        "dpadIcon",
        "indicatorIcon"
    };
}