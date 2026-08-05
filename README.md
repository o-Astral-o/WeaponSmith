# WeaponSmith
Simple WPF application for working with Call of Duty assets exported from [OpenAssetTools](https://github.com/Laupetin/OpenAssetTools)

## Supported file headers
- `WEAPONFILE`
- `ATTACHMENTFILE`
- `ATTACHMENTUNIQUEFILE`

These are really just a sanity check to make sure we don't parse garbage data,
you can support more formats by adding them to `%appdata%\WeaponSmith\config.cfg`.
However, keep in mind they must follow the [OpenAssetTools](https://github.com/Laupetin/OpenAssetTools) format as such: 

`HEADER\` followed by `\` separated key\value pairs
