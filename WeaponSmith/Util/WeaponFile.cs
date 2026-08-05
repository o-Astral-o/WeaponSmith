using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace WeaponSmith.Util;

public enum EntryCategory
{
    General,
    Damage,
    Model,
    Anim,
    StateTimers,
    Sound,
    FX,
    UI,
    Misc,
}

public class WeaponFile
{
    public string Header { get; set; } = string.Empty;
    public List<WeaponFileEntry> Entries { get; set; } = new();

    public WeaponFile(string path, IEnumerable<string> supportedHeaders)
    {
        Load(path, supportedHeaders);
    }

    public void Load(string path, IEnumerable<string> supportedHeaders)
    {
        var fileContents = File.ReadAllText(path);

        // Longest match first so a header that is a prefix of another can't shadow it
        Header = supportedHeaders
            .OrderByDescending(h => h.Length)
            .FirstOrDefault(h => fileContents.StartsWith(h))
            ?? throw new Exception("Unknown file type");

        var parts = fileContents.Split('\\').Skip(1).ToArray();
        
        // Initial loop
        for (int i = 0; i < parts.Length - 1; i += 2)
        {
            var key = parts[i];
            var value = parts[i + 1];
            
            var category = EntryCategorizer.Categorize(key);

            Entries.Add(new WeaponFileEntry(key, value, EntryCategorizer.Categorize(key)));
        }
        // Look for state timers
        foreach (var animEntry in Entries.Where(f => f.Category == EntryCategory.Anim))
        {
            var baseName = animEntry.Key.Replace("Anim", "", StringComparison.CurrentCultureIgnoreCase);
            foreach (var entry in Entries)
                if (entry.Key == baseName + "Time")
                    entry.Category = EntryCategory.StateTimers;
        }

        Console.WriteLine();
    }
}

public class WeaponFileEntry : INotifyPropertyChanged
{
    private string _key;
    private string _value;
    
    public string Key
    {
        get => _key;
        set
        {
            if (_key != value)
            {
                _key = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDropdownField));
                OnPropertyChanged(nameof(DropdownValues));
            }
        }
    }
    
    public string Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }
    
    public EntryCategory Category { get; set; }
    
    private static readonly Dictionary<string, List<string>> EnumTable = new()
    {
        { "weaponType", new List<string> { "bullet", "grenade", "projectile", "binoculars", "gas", "bomb", "mine", "melee", "riotshield" } },
        { "weapClass", new List<string> { "rifle", "mg", "smg", "spread", "pistol", "grenade", "rocketlauncher", "turret", "non-player", "gas", "item", "melee", "Killstreak Alt Stored Weapon", "pistol spread" } },
        { "penetrateType", new List<string> { "none", "small", "medium", "large" } },
        { "impactType", new List<string> { "none", "bullet_small", "bullet_large", "bullet_ap", "bullet_xtreme", "shotgun", "grenade_bounce", "grenade_explode", "rifle_grenade", "rocket_explode", "rocket_explode_xtreme", "projectile_dud", "mortar_shell", "tank_shell", "bolt", "blade" } },
        { "inventoryType", new List<string> { "primary", "offhand", "item", "altmode", "melee", "dwlefthand" } },
        { "fireType", new List<string> { "Full Auto", "Single Shot", "2-Round Burst", "3-Round Burst", "4-Round Burst", "5-Round Burst", "Stacked Fire", "Minigun", "Charge Shot", "Jet Gun" } },
        { "clipType", new List<string> { "bottom", "top", "left", "dp28", "ptrs", "lmg" } },
        { "barrelType", new List<string> { "Single", "Dual Barrel", "Dual Barrel Alternate", "Quad Barrel", "Quad Barrel Alternate", "Quad Barrel Double Alternate" } },
        { "offhandClass", new List<string> { "None", "Frag Grenade", "Smoke Grenade", "Flash Grenade", "Gear", "Supply Drop Marker" } },
        { "offhandSlot", new List<string> { "None", "Lethal Grenade", "Tactical Grenade", "Equipment", "Specific Use" } },
        { "stance", new List<string> { "stand", "duck", "prone" } },
        { "reticleType", new List<string> { "None", "Pip-On-A-Stick", "Bouncing Diamond", "Missile Lock" } },
        { "weaponIconRatioType", new List<string> { "1:1", "2:1", "4:1" } },
        { "ammoCounterClip", new List<string> { "None", "Magazine", "ShortMagazine", "Shotgun", "Rocket", "BeltFed", "AltWeapon" } },
        { "overlayReticle", new List<string> { "none", "crosshair" } },
        { "overlayInterface", new List<string> { "None", "Javelin", "Turret Scope" } },
        { "projExplosionType", new List<string> { "grenade", "rocket", "flashbang", "none", "dud", "smoke", "heavy explosive", "fire", "napalmblob", "bolt", "shrapnel span" } },
        { "stickiness", new List<string> { "Don't stick", "Stick to all", "Stick to all, except ai and clients", "Stick to ground", "Stick to ground, maintain yaw", "Stick to flesh" } },
        { "rotateType", new List<string> { "Rotate both axis, grenade style", "Rotate one axis, blade style", "Rotate like a cylinder" } },
        { "guidedMissileType", new List<string> { "None", "Sidewinder", "Hellfire", "Javelin", "Ballistic", "WireGuided", "TVGuided", "Drone", "HeatSeeking" } },
    };
    
    public bool IsDropdownField => EnumTable.ContainsKey(Key);
    
    public IEnumerable<string>? DropdownValues => IsDropdownField ? EnumTable[Key] : null;

    public WeaponFileEntry(string key, string value, EntryCategory category = EntryCategory.Misc)
    {
        _key = key;
        _value = value;
        Category = category;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}