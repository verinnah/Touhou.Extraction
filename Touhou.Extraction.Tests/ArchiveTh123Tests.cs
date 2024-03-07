using System.Collections.Frozen;
using System.IO.Hashing;
using System.Text;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh123Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "data/character/alice/BulletAa007.cv2", 0x12dacb8989fda59f },
		{ "data/character/alice/BulletAb007.cv2", 0xe20bf3c113757771 },
		{ "data/character/alice/BulletAc007.cv2", 0x9cdbdf139045c3eb },
		{ "data/character/alice/alice.pat", 0xf302a72ed93d9f23 },
		{ "data/character/alice/attackBf000.cv2", 0x26850e628b49346f },
		{ "data/character/alice/attackBf002.cv2", 0xef915e451aedada7 },
		{ "data/character/alice/attackBf003.cv2", 0xc6719b107f69381f },
		{ "data/character/alice/attackCa004.cv2", 0x8a14a9bddba92aac },
		{ "data/character/alice/attackCa005.cv2", 0xe35a9e02ea2c8c32 },
		{ "data/character/alice/attackCa006.cv2", 0x5d1eced1e31a13d1 },
		{ "data/character/alice/attackCa007.cv2", 0xaf0a3d8082747da9 },
		{ "data/character/alice/attackCa010.cv2", 0xa93a44a02809481c },
		{ "data/character/alice/hitSit000.cv2", 0x3ee9249934b8f69f },
		{ "data/character/alice/hitUpper000.cv2", 0xfe962baaaadc008d },
		{ "data/character/alice/jump007.cv2", 0x9f352d8049bdce38 },
		{ "data/character/alice/objectAo000.cv2", 0xb3e8bd68639d7fa8 },
		{ "data/character/alice/objectAo001.cv2", 0xc2702d7abdd560d1 },
		{ "data/character/alice/objectAo002.cv2", 0x299a2941370b29c3 },
		{ "data/character/alice/objectAo003.cv2", 0x1d30a36a43fff0e6 },
		{ "data/character/alice/objectAo004.cv2", 0x2d38b966a56c139f },
		{ "data/character/alice/objectAo005.cv2", 0x52b75fc8bea6a70f },
		{ "data/character/alice/palette002.pal", 0x82d2be1d8fbfe64a },
		{ "data/character/alice/shotBa012.cv2", 0xac5610a13f47abef },
		{ "data/character/alice/shotCa001.cv2", 0x52ce1e11a3a40b9e },
		{ "data/character/alice/shotCa005.cv2", 0x555f4d0ce930650 },
		{ "data/character/alice/spellAa004.cv2", 0x9e12bfb68b9fce9e },
		{ "data/character/alice/spellAa005.cv2", 0x31358e572e23d804 },
		{ "data/character/alice/spellAb000.cv2", 0xdab7efb3b1453e98 },
		{ "data/character/alice/spellAb001.cv2", 0x14964fa1f501ebe9 },
		{ "data/character/alice/spellAb002.cv2", 0x7c94b0ec232e4f8e },
		{ "data/character/alice/spellAb003.cv2", 0xac41b6b96ef62515 },
		{ "data/character/alice/spellAb004.cv2", 0x8751c2bd07b0f981 },
		{ "data/character/alice/spellAb005.cv2", 0xaacbe4a720aad3be },
		{ "data/character/alice/spellAb006.cv2", 0x5ef655c515c7d28e },
		{ "data/character/alice/spellAb007.cv2", 0xfca576996a2c724a },
		{ "data/character/alice/spellAb008.cv2", 0x1b936f9b7016caff },
		{ "data/character/alice/spellAb009.cv2", 0x79daad27a74c0037 },
		{ "data/character/alice/spellEa008.cv2", 0x912c295f75e5f9f1 },
		{ "data/character/aya/aya.pat", 0x104a89533aaad91e },
		{ "data/character/aya/tailAa000.cv2", 0x3e91c9bc5cd190fd },
		{ "data/character/chirno/back/spell000.cv2", 0xedbbfd2f89dd5e91 },
		{ "data/character/chirno/chirno.pat", 0x95b989964b4a8b7b },
		{ "data/character/chirno/hitAir003.cv2", 0x4c467c64ac304683 },
		{ "data/character/chirno/spellAb001.cv2", 0x95f4de36e13b217c },
		{ "data/character/chirno/spellAb002.cv2", 0xd3ddec9efc0fe50a },
		{ "data/character/chirno/spellAb003.cv2", 0x7d7078767b798687 },
		{ "data/character/chirno/spellAb004.cv2", 0x7cc76232bda903e9 },
		{ "data/character/chirno/spellAb005.cv2", 0x5e1c3e201a76d13b },
		{ "data/character/chirno/spellAb006.cv2", 0xb5bed46fc9fcac36 },
		{ "data/character/chirno/spellAb007.cv2", 0xc954d09d6c6cfcf },
		{ "data/character/chirno/spellAb008.cv2", 0x72ce102cfa9997c9 },
		{ "data/character/chirno/spellAb009.cv2", 0xab6f174c7211974b },
		{ "data/character/common/common.pat", 0xa203ba14cc370eb1 },
		{ "data/character/iku/attackAc000.cv2", 0x6bee91ed056fce68 },
		{ "data/character/iku/attackBe007.cv2", 0x7e828637975fc5db },
		{ "data/character/iku/dashBack001.cv2", 0xb56d0fd157102f59 },
		{ "data/character/iku/hitUnder002.cv2", 0xb000e37a81fb5da0 },
		{ "data/character/iku/iku.pat", 0x3e36ea462e4246a6 },
		{ "data/character/iku/jump003.cv2", 0x8c4ba73ad3a52cdc },
		{ "data/character/iku/objectBa002.cv2", 0x4bd95684c2e9943e },
		{ "data/character/iku/objectBa003.cv2", 0x3a2a16d35d93b352 },
		{ "data/character/iku/objectBa004.cv2", 0x436a8f65c24bf59b },
		{ "data/character/iku/objectBa005.cv2", 0x1557e3749e64988a },
		{ "data/character/iku/objectBa006.cv2", 0x2039b3b052d2318b },
		{ "data/character/iku/objectBa007.cv2", 0xef6c4260d3feac0c },
		{ "data/character/iku/objectBa008.cv2", 0x87496dd90a8b43b2 },
		{ "data/character/iku/objectBa009.cv2", 0xdc7bcd22fdbc7758 },
		{ "data/character/iku/objectBa010.cv2", 0x12b540fdb4ecd9fb },
		{ "data/character/iku/objectBa011.cv2", 0xb2b1a4e6dadcb745 },
		{ "data/character/iku/objectBa012.cv2", 0x7a649ca5698e75b1 },
		{ "data/character/iku/objectBa013.cv2", 0x7cce16e2f2315d12 },
		{ "data/character/iku/objectBa014.cv2", 0xf9fb89caef526f47 },
		{ "data/character/iku/objectBb000.cv2", 0x9a659499afa838ff },
		{ "data/character/iku/objectBb001.cv2", 0x7220b8ac890e49ac },
		{ "data/character/iku/objectBb002.cv2", 0x54e166266180ee17 },
		{ "data/character/iku/objectBb003.cv2", 0xc49c68a2d390c703 },
		{ "data/character/iku/objectBb004.cv2", 0xa956b6f1317d4ef0 },
		{ "data/character/iku/objectBb005.cv2", 0x142b77a0f4f62b0e },
		{ "data/character/iku/objectBb006.cv2", 0xb53ca1c3a6fd67a },
		{ "data/character/iku/objectBb007.cv2", 0x3f16ecba5e559bc3 },
		{ "data/character/iku/spellBa006.cv2", 0xd96daeff9b15d4c5 },
		{ "data/character/iku/spellBa007.cv2", 0x57541685177be564 },
		{ "data/character/iku/spellDa010.cv2", 0x3190466c9baee2ac },
		{ "data/character/iku/spellEa000.cv2", 0xa67bf575cdc23ef0 },
		{ "data/character/iku/spellEa001.cv2", 0xf6dd3c7262a96013 },
		{ "data/character/iku/spellEa002.cv2", 0x316cb5437522cf87 },
		{ "data/character/iku/spellEa004.cv2", 0x9c3685bd46c7756b },
		{ "data/character/iku/spellEa006.cv2", 0x50be6938b8e15d77 },
		{ "data/character/iku/spellEa007.cv2", 0xedd393275ec34527 },
		{ "data/character/iku/walkBack000.cv2", 0xe84571eb9aa26897 },
		{ "data/character/iku/walkBack001.cv2", 0x301b912cf0472e8a },
		{ "data/character/iku/walkBack002.cv2", 0x6246e1529b218649 },
		{ "data/character/iku/walkBack003.cv2", 0x7aa1f6b6a11a9ad6 },
		{ "data/character/iku/walkFront002.cv2", 0x63eb272bac76955 },
		{ "data/character/iku/walkFrontB000.cv2", 0x9a3e30431749eff9 },
		{ "data/character/iku/walkFrontB001.cv2", 0x9a2ba0592bd6eeb1 },
		{ "data/character/iku/walkFrontB002.cv2", 0xde749ec7c6d0b55f },
		{ "data/character/iku/walkFrontB003.cv2", 0x912d614bd5cbdd3b },
		{ "data/character/komachi/bulletDb000.cv2", 0x274ae90abce22bb5 },
		{ "data/character/komachi/bulletDb001.cv2", 0x3ffbf8b0d81e3745 },
		{ "data/character/komachi/bulletDb002.cv2", 0x51f66704093876b8 },
		{ "data/character/komachi/bulletDb003.cv2", 0xf676da26861e7278 },
		{ "data/character/komachi/bulletDb004.cv2", 0xbcb4c9d72a157ce },
		{ "data/character/komachi/bulletDb005.cv2", 0x5d2d1e4a143f42ed },
		{ "data/character/komachi/bulletDb006.cv2", 0x3ea1fbebe3542929 },
		{ "data/character/komachi/bulletDb007.cv2", 0x652daf7a0b4df1a7 },
		{ "data/character/komachi/bulletDb008.cv2", 0xef8f38c05427d235 },
		{ "data/character/komachi/bulletDb009.cv2", 0x52132e71842f6000 },
		{ "data/character/komachi/bulletDb010.cv2", 0xa86d235c96da8379 },
		{ "data/character/komachi/bulletDb011.cv2", 0x231f553f698f71b2 },
		{ "data/character/komachi/bulletDb012.cv2", 0x8fed341bef9fe591 },
		{ "data/character/komachi/bulletDb013.cv2", 0x74a4217a307a9249 },
		{ "data/character/komachi/bulletDb014.cv2", 0x922a31743e2d839c },
		{ "data/character/komachi/bulletDb015.cv2", 0xf7858ffec950cd4b },
		{ "data/character/komachi/bulletDb016.cv2", 0x895615bdb08fecf2 },
		{ "data/character/komachi/bulletDb017.cv2", 0xe65cdda00697324c },
		{ "data/character/komachi/bulletDb018.cv2", 0x526cca1f34422d09 },
		{ "data/character/komachi/bulletDb019.cv2", 0x634505863036326 },
		{ "data/character/komachi/bulletDb020.cv2", 0xcd1530e850b10c3a },
		{ "data/character/komachi/bulletDb021.cv2", 0x4146881eff9e7a69 },
		{ "data/character/komachi/bulletDb022.cv2", 0x7314c1e5f15f938d },
		{ "data/character/komachi/bulletDb023.cv2", 0x9916dc6ee6f56bf3 },
		{ "data/character/komachi/bulletDb024.cv2", 0x4bec49b1ca8520b9 },
		{ "data/character/komachi/bulletDb025.cv2", 0xa9bf9eaac31029b3 },
		{ "data/character/komachi/bulletDb026.cv2", 0xaafa26e188043767 },
		{ "data/character/komachi/bulletDb027.cv2", 0x96f9694ff4678b5b },
		{ "data/character/komachi/bulletDb028.cv2", 0x66a33b4039b12647 },
		{ "data/character/komachi/bulletDb029.cv2", 0xbe5fb9307696b0e5 },
		{ "data/character/komachi/bulletDb030.cv2", 0x1df240b5957c0ba4 },
		{ "data/character/komachi/bulletDb031.cv2", 0x8939f676858be97a },
		{ "data/character/komachi/bulletDb032.cv2", 0xce59ce90ac642f4a },
		{ "data/character/komachi/dashAirFront002.cv2", 0x5d1719a38abb30fc },
		{ "data/character/komachi/dashFront009.cv2", 0x473c1065000e4973 },
		{ "data/character/komachi/komachi.pat", 0xb2ae15278416dc32 },
		{ "data/character/komachi/shotBa005.cv2", 0xa7279851aae489 },
		{ "data/character/komachi/shotBa006.cv2", 0xd2620d9d256de4a8 },
		{ "data/character/komachi/sit008.cv2", 0xab4f1ad8294f70a6 },
		{ "data/character/komachi/spellBa015.cv2", 0xee02cd2f5b5aeec1 },
		{ "data/character/komachi/spellBulletFa000.cv2", 0x3fd3907e30399ba5 },
		{ "data/character/komachi/spellBulletFa001.cv2", 0x655e59abfe68d1e7 },
		{ "data/character/komachi/spellBulletFa002.cv2", 0x245155f4ebe085f8 },
		{ "data/character/komachi/spellBulletFa003.cv2", 0x629e7ba0d30e2717 },
		{ "data/character/komachi/spellBulletFa004.cv2", 0x5f5f8033c3470885 },
		{ "data/character/komachi/spellBulletFa005.cv2", 0x7854559a2cb69b98 },
		{ "data/character/komachi/spellBulletFa006.cv2", 0x8b86ae94750df616 },
		{ "data/character/komachi/spellBulletFa007.cv2", 0xb925203e42bc70a0 },
		{ "data/character/komachi/spellBulletFa008.cv2", 0xd9ebed195992f7ca },
		{ "data/character/komachi/spellBulletFa009.cv2", 0xf640985bb0dc0ed3 },
		{ "data/character/komachi/spellBulletFa010.cv2", 0x42102b3c705eaec8 },
		{ "data/character/komachi/spellBulletFa011.cv2", 0xe9ba3434155fc974 },
		{ "data/character/komachi/spellBulletFa012.cv2", 0xa8917c710cc48d63 },
		{ "data/character/komachi/spellBulletFa013.cv2", 0x66c36ce6329d6e17 },
		{ "data/character/komachi/spellBulletFa014.cv2", 0xe829ebcbabb5922f },
		{ "data/character/komachi/spellBulletFa015.cv2", 0x985563b3d1a1ffa9 },
		{ "data/character/komachi/spellEa015.cv2", 0x267b1f4f6c69dd3f },
		{ "data/character/komachi/walkBack000.cv2", 0xdf7eb5852f04dbdb },
		{ "data/character/komachi/walkBack001.cv2", 0xa21e83a1f70e5b74 },
		{ "data/character/komachi/walkBack002.cv2", 0x7ad86666d1fe6092 },
		{ "data/character/komachi/walkBack003.cv2", 0x11a69a783a476848 },
		{ "data/character/komachi/walkBack004.cv2", 0x5f29e7dff9ee5596 },
		{ "data/character/komachi/walkBack005.cv2", 0x36b520bbbc40fd0 },
		{ "data/character/komachi/walkBack006.cv2", 0xc77adcf62afa81ab },
		{ "data/character/komachi/walkBack008.cv2", 0xc6f7f41955c7a2e5 },
		{ "data/character/komachi/walkBack009.cv2", 0xf99be3d6daf43cd7 },
		{ "data/character/komachi/walkBack010.cv2", 0xf20a7c920f962e3e },
		{ "data/character/komachi/walkBack011.cv2", 0x9e2b97151eb5190b },
		{ "data/character/komachi/walkBack012.cv2", 0x87bff1df8a430180 },
		{ "data/character/komachi/walkBack013.cv2", 0x482c66a826aef020 },
		{ "data/character/komachi/walkBack014.cv2", 0x749e16b5e32a9073 },
		{ "data/character/marisa/attackAb002.cv2", 0x199c73730fc1ba05 },
		{ "data/character/marisa/attackAb003.cv2", 0x8d7dc6b745d5d0f0 },
		{ "data/character/marisa/attackAb004.cv2", 0x59ccac63b5d45f7b },
		{ "data/character/marisa/attackCb005.cv2", 0x20f6f0d6eb44feb0 },
		{ "data/character/marisa/attackCb006.cv2", 0xe9234e5177acdb0a },
		{ "data/character/marisa/hitSpin000.cv2", 0xb354b0da3afa3fc0 },
		{ "data/character/marisa/jump005.cv2", 0x8d0d266c0af6736e },
		{ "data/character/marisa/jumpFront009.cv2", 0xbe632363ec33631e },
		{ "data/character/marisa/marisa.pat", 0x5fc411341c8d0549 },
		{ "data/character/marisa/palette007.pal", 0xcf6b14755fe9bed6 },
		{ "data/character/marisa/shotAc001.cv2", 0xca63ac7b466ac5f3 },
		{ "data/character/marisa/shotAc002.cv2", 0x8046a4b014f02c41 },
		{ "data/character/marisa/shotAc003.cv2", 0x4e9dbcb0f1774ca9 },
		{ "data/character/marisa/shotAc004.cv2", 0x665d64c62f32c549 },
		{ "data/character/marisa/shotAc005.cv2", 0xb8dac4eafdfcd952 },
		{ "data/character/marisa/shotAc006.cv2", 0x630e4b8f29ce05d6 },
		{ "data/character/marisa/shotAc007.cv2", 0x13d2e77ad1596065 },
		{ "data/character/marisa/shotAc008.cv2", 0xb1f5ff62c34e0996 },
		{ "data/character/marisa/shotAc009.cv2", 0xa32efa16d7d5cbd0 },
		{ "data/character/marisa/shotBa013.cv2", 0xf51d2318a59165b8 },
		{ "data/character/marisa/shotBa014.cv2", 0xcfeb34457aac6775 },
		{ "data/character/marisa/shotBb007.cv2", 0x2b5ea3b9141e17c9 },
		{ "data/character/marisa/shotBb008.cv2", 0x597591fe143eeaa2 },
		{ "data/character/marisa/spellAa005.cv2", 0xdef080bfb7365fc },
		{ "data/character/marisa/spellBa004.cv2", 0x38afe9b4087399a1 },
		{ "data/character/marisa/spellBb003.cv2", 0x6bb2749d85ac9518 },
		{ "data/character/marisa/spellBulletHa000.cv2", 0x52771e4335e4a5d3 },
		{ "data/character/marisa/spellBulletHa001.cv2", 0x4cf7385548d92612 },
		{ "data/character/marisa/spellBulletHa002.cv2", 0xe33e74d998ba1ac6 },
		{ "data/character/marisa/spellBulletHa003.cv2", 0x705c53e22a441f96 },
		{ "data/character/marisa/spellBulletHa004.cv2", 0x9ba2c3876903b29b },
		{ "data/character/marisa/spellBulletHa005.cv2", 0xefbd8c6e7f0ca400 },
		{ "data/character/marisa/spellBulletHa006.cv2", 0x70ed9de79882374a },
		{ "data/character/marisa/spellBulletHa007.cv2", 0x972fa84586544d21 },
		{ "data/character/marisa/spellBulletHa008.cv2", 0x58e08a205599d51 },
		{ "data/character/marisa/spellBulletHa009.cv2", 0xf2fcd5717043eb5e },
		{ "data/character/marisa/spellBulletHa010.cv2", 0x18d8891ecb57bac6 },
		{ "data/character/marisa/spellBulletHa011.cv2", 0xbf7fe2fdd3adb834 },
		{ "data/character/marisa/spellBulletHa012.cv2", 0x298ad6eb5539ebf },
		{ "data/character/marisa/spellBulletHa013.cv2", 0x35bf706aab64286 },
		{ "data/character/marisa/spellBulletHa014.cv2", 0x482cee86a66351f },
		{ "data/character/marisa/spellBulletHa015.cv2", 0xa0ec9bc49bbc4b9b },
		{ "data/character/marisa/spellCa005.cv2", 0x54d1c242ed346d26 },
		{ "data/character/marisa/spellCa006.cv2", 0xe242b75cdf0be6e3 },
		{ "data/character/marisa/spellCa007.cv2", 0x2aa7c4200fefa5f8 },
		{ "data/character/marisa/spellDa000.cv2", 0x858d06fa2f5d6fcf },
		{ "data/character/marisa/spellDa001.cv2", 0xce39bec1f92c4c3a },
		{ "data/character/marisa/spellDa002.cv2", 0xfe2f14326e905687 },
		{ "data/character/marisa/spellDa003.cv2", 0x17a6bbd545494df4 },
		{ "data/character/marisa/spellDa004.cv2", 0x16fcdcfa2db3df59 },
		{ "data/character/marisa/spellDb004.cv2", 0x4970921f065f1278 },
		{ "data/character/marisa/spellDb005.cv2", 0x22654dc68224e88d },
		{ "data/character/marisa/spellEd000.cv2", 0xf92a27158784c640 },
		{ "data/character/marisa/spellGa004.cv2", 0x7d5e8e3d48e5b1ba },
		{ "data/character/marisa/spellGa006.cv2", 0xe9684c669957f0da },
		{ "data/character/marisa/spellGb001.cv2", 0xdaa027dfb618cf71 },
		{ "data/character/marisa/spellHa001.cv2", 0xd7c715ba6d2b1aca },
		{ "data/character/marisa/spellHa003.cv2", 0xf8fa56aae0198eaa },
		{ "data/character/marisa/spellHa007.cv2", 0x49352a8847a77c98 },
		{ "data/character/marisa/spellHa008.cv2", 0xa1a376ff67dcb268 },
		{ "data/character/marisa/spellHa009.cv2", 0x895f693946b71ece },
		{ "data/character/marisa/walkBack002.cv2", 0xf5e010842861971c },
		{ "data/character/marisa/walkFront000.cv2", 0xc13dd897d2777162 },
		{ "data/character/marisa/walkFront001.cv2", 0xd5ae4d7ecaaf352f },
		{ "data/character/marisa/walkFront002.cv2", 0x6c7f5db34a65f92 },
		{ "data/character/marisa/walkFront003.cv2", 0xe12ec6a693fa87ee },
		{ "data/character/marisa/walkFront004.cv2", 0x4719bf9905ce4644 },
		{ "data/character/marisa/walkFront005.cv2", 0xd07d7567f8d37f4d },
		{ "data/character/marisa/walkFront006.cv2", 0xfbe9b990360a3a25 },
		{ "data/character/marisa/walkFront007.cv2", 0xb2a44b3cfb3d8c07 },
		{ "data/character/meirin/attackAa003.cv2", 0xc140f6a79888e1e8 },
		{ "data/character/meirin/attackCc002.cv2", 0xc5dab9e62a5ab6aa },
		{ "data/character/meirin/back/spell000.cv2", 0x1fd896bbd20c86fe },
		{ "data/character/meirin/dashBack001.cv2", 0xd6bfe2484a25c041 },
		{ "data/character/meirin/dashBack002.cv2", 0x4b0c812c36ab0741 },
		{ "data/character/meirin/meirin.pat", 0x870aea317ddf9907 },
		{ "data/character/meirin/spellCall001.cv2", 0xf9f2d621f10b9b5 },
		{ "data/character/meirin/spellCall007.cv2", 0x5470adf71cad0d53 },
		{ "data/character/meirin/spellCb000.cv2", 0xca5a18f49c0639a9 },
		{ "data/character/meirin/spellCb001.cv2", 0x5b748a34e39f737 },
		{ "data/character/meirin/spellCb002.cv2", 0x2aa0b75f661f60b0 },
		{ "data/character/meirin/spellCb003.cv2", 0x19db7ab0922405a0 },
		{ "data/character/meirin/spellCb004.cv2", 0x36358bf938a42d73 },
		{ "data/character/meirin/spellCb005.cv2", 0x6ac872bcb6a6b782 },
		{ "data/character/meirin/spellCb006.cv2", 0x5559d908fba9d4a5 },
		{ "data/character/meirin/spellCb007.cv2", 0x35880f4ceb1723b5 },
		{ "data/character/meirin/spellCb008.cv2", 0x72603b8bf0be0b81 },
		{ "data/character/meirin/spellCb009.cv2", 0x729b1600320b26b7 },
		{ "data/character/meirin/spellCb010.cv2", 0xd9ac4fd34a786a26 },
		{ "data/character/meirin/spellCb011.cv2", 0x12718ae2c1e54ecf },
		{ "data/character/meirin/spellCb012.cv2", 0x70f9b282ccea55ff },
		{ "data/character/meirin/spellCb013.cv2", 0x12718ae2c1e54ecf },
		{ "data/character/meirin/spellCb014.cv2", 0x93cf75b73997c962 },
		{ "data/character/meirin/spellCb015.cv2", 0x4c399535b167644 },
		{ "data/character/patchouli/attackAa002.cv2", 0x9fd55846d0d3d90f },
		{ "data/character/patchouli/attackAa003.cv2", 0x40175ef682017633 },
		{ "data/character/patchouli/attackAb002.cv2", 0xcefe7e6bb7330fe0 },
		{ "data/character/patchouli/attackAc001.cv2", 0xa66467bad8ec064e },
		{ "data/character/patchouli/attackAc002.cv2", 0x8912978efc4bf090 },
		{ "data/character/patchouli/attackAc005.cv2", 0x1dced88608ad377b },
		{ "data/character/patchouli/attackAc006.cv2", 0x48750f344d0541f8 },
		{ "data/character/patchouli/attackBa007.cv2", 0x1a088293c00ed840 },
		{ "data/character/patchouli/attackBa008.cv2", 0x8e05a98dd5ea2f5d },
		{ "data/character/patchouli/attackBa009.cv2", 0x17d45ba50fc2bb83 },
		{ "data/character/patchouli/attackBa010.cv2", 0xb7cd8b069bf47036 },
		{ "data/character/patchouli/attackBa012.cv2", 0x2ccc08ea1eabee93 },
		{ "data/character/patchouli/attackCa005.cv2", 0xaf94a6f7458ae13f },
		{ "data/character/patchouli/attackCa006.cv2", 0xe690d9c8fd2f5057 },
		{ "data/character/patchouli/attackCa007.cv2", 0x57518a1c733997a },
		{ "data/character/patchouli/attackCa008.cv2", 0x11dc014271894555 },
		{ "data/character/patchouli/attackCa009.cv2", 0x3f6153787dbf506 },
		{ "data/character/patchouli/attackCa010.cv2", 0xd09bdeaee23a3ff0 },
		{ "data/character/patchouli/crash000.cv2", 0x21586411c21482c1 },
		{ "data/character/patchouli/crash001.cv2", 0x5fd646aa57cb3a84 },
		{ "data/character/patchouli/dashBack006.cv2", 0x3f60002225b2170a },
		{ "data/character/patchouli/dashFront002.cv2", 0x133e70784e625d5b },
		{ "data/character/patchouli/dashFront003.cv2", 0x8ec24c5be3cb61fe },
		{ "data/character/patchouli/dashFront004.cv2", 0x5a4fa3017b925e3d },
		{ "data/character/patchouli/dashFront005.cv2", 0x6b43bc8651d7fbec },
		{ "data/character/patchouli/jump001.cv2", 0x6e4c7758be754883 },
		{ "data/character/patchouli/jump009.cv2", 0xfc847d03cad4b2f0 },
		{ "data/character/patchouli/patchouli.pat", 0xe9e7fd0e05756f35 },
		{ "data/character/patchouli/shotAa000.cv2", 0x4c30179b9359c456 },
		{ "data/character/patchouli/shotAc000.cv2", 0xe5ce93a54da65894 },
		{ "data/character/patchouli/shotAc001.cv2", 0xc32ad5682c3955 },
		{ "data/character/patchouli/shotAc002.cv2", 0xb8918d83a6c3edc5 },
		{ "data/character/patchouli/shotAc010.cv2", 0x5d1dbf9be6f7d3f6 },
		{ "data/character/patchouli/shotBa003.cv2", 0x2c7a3c508dd8019a },
		{ "data/character/patchouli/shotBa004.cv2", 0xedda8410bcb7e4b2 },
		{ "data/character/patchouli/shotBb001.cv2", 0x2aace443acf9955d },
		{ "data/character/patchouli/spellAa004.cv2", 0x598a846d8347f9f3 },
		{ "data/character/patchouli/spellAa010.cv2", 0x92ba79c820a9f066 },
		{ "data/character/patchouli/spellAa011.cv2", 0xc2dbf702a30552ae },
		{ "data/character/patchouli/spellAa012.cv2", 0xafcff73d3c3d724a },
		{ "data/character/patchouli/spellAa013.cv2", 0x15643bcbc8837bd6 },
		{ "data/character/patchouli/spellAa014.cv2", 0xe53a162a47158d69 },
		{ "data/character/patchouli/spellCa002.cv2", 0x316faf344e410993 },
		{ "data/character/patchouli/spellCa004.cv2", 0xe7bd37e605c6e8dd },
		{ "data/character/patchouli/spellCa005.cv2", 0x55198fc6554836bd },
		{ "data/character/patchouli/spellCa006.cv2", 0x366ed97de6395dbd },
		{ "data/character/patchouli/spellCa007.cv2", 0x2d2a4d20c3dd1820 },
		{ "data/character/patchouli/spellCa008.cv2", 0x64b4196fc0b10b97 },
		{ "data/character/patchouli/spellCa009.cv2", 0x950068c171d428f2 },
		{ "data/character/patchouli/spellCa011.cv2", 0xa1671d407f99a185 },
		{ "data/character/patchouli/spellCall002.cv2", 0x1e31be46e17b1edb },
		{ "data/character/patchouli/spellCall004.cv2", 0x10e55b578691142c },
		{ "data/character/patchouli/spellCall007.cv2", 0xc742a820d879847f },
		{ "data/character/patchouli/spellCall008.cv2", 0xd48d750f016c1a6b },
		{ "data/character/patchouli/spellCall009.cv2", 0x42f53c6ee26f6663 },
		{ "data/character/patchouli/spellDa000.cv2", 0xc6f1c9fdf554c79a },
		{ "data/character/patchouli/spellDa001.cv2", 0x7c409680f8a2cbd4 },
		{ "data/character/patchouli/spellDa015.cv2", 0x659bc82e3ba2e8b1 },
		{ "data/character/patchouli/spellDa016.cv2", 0x1d005e1105d26007 },
		{ "data/character/patchouli/spellDa017.cv2", 0xc1e29e295abb39ee },
		{ "data/character/patchouli/spellEa002.cv2", 0x4ade551f16b441ec },
		{ "data/character/patchouli/spellEa005.cv2", 0xa0b0a8c1d560ba49 },
		{ "data/character/patchouli/spellEa008.cv2", 0x8bfe095d5d69c1f2 },
		{ "data/character/patchouli/spellEa010.cv2", 0xd6a823d8107bffc8 },
		{ "data/character/patchouli/spellFa001.cv2", 0x46077f3104ebfe66 },
		{ "data/character/patchouli/spellFa002.cv2", 0xd089b2810d6bdd9a },
		{ "data/character/patchouli/spellGa002.cv2", 0x61294e5a8027b634 },
		{ "data/character/patchouli/spellGa003.cv2", 0x595164a5745a39e1 },
		{ "data/character/patchouli/spellGa004.cv2", 0x3768eecbabb6f651 },
		{ "data/character/patchouli/spellGa005.cv2", 0x4fa69ac67fcbd46f },
		{ "data/character/patchouli/spellGa006.cv2", 0xe4be866dce170255 },
		{ "data/character/patchouli/spellHa000.cv2", 0x8a2cee3c577113f7 },
		{ "data/character/patchouli/spellHa001.cv2", 0x4eda67b2eb8c268f },
		{ "data/character/patchouli/spellHa002.cv2", 0x4af1d1ba88bc1527 },
		{ "data/character/patchouli/spellHa003.cv2", 0xb1536b765ee24c56 },
		{ "data/character/patchouli/spellIa002.cv2", 0x72f3e3473454fd35 },
		{ "data/character/patchouli/spellIa003.cv2", 0x8d0f2770b7c67cad },
		{ "data/character/patchouli/spellKa001.cv2", 0x427632468d8415b7 },
		{ "data/character/patchouli/spellKa007.cv2", 0xc7073810672a182b },
		{ "data/character/patchouli/spellKa015.cv2", 0xd3073603dd75ba18 },
		{ "data/character/patchouli/stand008.cv2", 0x40c70d340178ec45 },
		{ "data/character/patchouli/stand012.cv2", 0xb2e1543127fe5efa },
		{ "data/character/patchouli/stand013.cv2", 0xb74df4f1a87c203f },
		{ "data/character/patchouli/standUp002.cv2", 0x56fa21bac6a0a7d5 },
		{ "data/character/patchouli/standUp003.cv2", 0xb9a787511deb4830 },
		{ "data/character/patchouli/standUp004.cv2", 0x8f0e16a1b57c062e },
		{ "data/character/reimu/ObjectAc000.cv2", 0x6006cdfb43b52480 },
		{ "data/character/reimu/ObjectAc001.cv2", 0xcb04fea7625f8284 },
		{ "data/character/reimu/ObjectAc002.cv2", 0xd1b974e8bf2ff78 },
		{ "data/character/reimu/ObjectAc003.cv2", 0xb460a748b6c9b1ed },
		{ "data/character/reimu/ObjectAc004.cv2", 0x5032264abdcff318 },
		{ "data/character/reimu/ObjectAc005.cv2", 0x5031b010e2ce03f0 },
		{ "data/character/reimu/ObjectAc006.cv2", 0xfaf4db0591dcc7e2 },
		{ "data/character/reimu/ObjectAc007.cv2", 0xefe17489edfc830f },
		{ "data/character/reimu/ObjectAc008.cv2", 0x26a597de9e5d7ae1 },
		{ "data/character/reimu/ObjectAc009.cv2", 0xf46aafcafe1ab8f9 },
		{ "data/character/reimu/ObjectAc010.cv2", 0x9398e1fed51e613c },
		{ "data/character/reimu/ObjectAc011.cv2", 0x4f3619c771d3b2a8 },
		{ "data/character/reimu/ObjectAc012.cv2", 0x298dbad7431f6fb4 },
		{ "data/character/reimu/ObjectAc013.cv2", 0x2be53d080c1dca0b },
		{ "data/character/reimu/ObjectAc014.cv2", 0x9d32245e714837d2 },
		{ "data/character/reimu/ObjectAc015.cv2", 0x518327598b8d1839 },
		{ "data/character/reimu/ObjectAc016.cv2", 0xf48bab20e5798148 },
		{ "data/character/reimu/ObjectAc017.cv2", 0x4fa66be0b3fb7601 },
		{ "data/character/reimu/ObjectAd000.cv2", 0xc946544591de67ac },
		{ "data/character/reimu/ObjectAd001.cv2", 0x83dd2c2e3060c89d },
		{ "data/character/reimu/ObjectAd002.cv2", 0xa635cd1af3400c45 },
		{ "data/character/reimu/ObjectAd003.cv2", 0x8c46796e896969ed },
		{ "data/character/reimu/attackA002.cv2", 0xc22a87e7d9fe3029 },
		{ "data/character/reimu/attackA003.cv2", 0x3f6f9fb151df8a1e },
		{ "data/character/reimu/attackA005.cv2", 0x5c87109efa447bf9 },
		{ "data/character/reimu/attackC000.cv2", 0x2328b5b49dc51b33 },
		{ "data/character/reimu/attackC004.cv2", 0x3d58903c2e28b8d7 },
		{ "data/character/reimu/attackD001.cv2", 0xf5120a1799b26c5c },
		{ "data/character/reimu/attackD002.cv2", 0xd9d114c245545635 },
		{ "data/character/reimu/attackD003.cv2", 0xcc7d91dff11fc45d },
		{ "data/character/reimu/attackD004.cv2", 0x463deb147595c99e },
		{ "data/character/reimu/attackD005.cv2", 0xbe78dc591377bba7 },
		{ "data/character/reimu/attackD006.cv2", 0x8dfc9390143e8472 },
		{ "data/character/reimu/attackD007.cv2", 0xc043b1efd4b57979 },
		{ "data/character/reimu/attackD008.cv2", 0xbe78dc591377bba7 },
		{ "data/character/reimu/attackE001.cv2", 0x5897e764dfd54d42 },
		{ "data/character/reimu/attackE002.cv2", 0x77ebd37e6a3de509 },
		{ "data/character/reimu/attackE003.cv2", 0x2f29e9def553a036 },
		{ "data/character/reimu/attackE004.cv2", 0x613739f7f53a4dda },
		{ "data/character/reimu/attackH000.cv2", 0x2937b5f6a1cd3f5e },
		{ "data/character/reimu/attackH002.cv2", 0x71ded826a54052fc },
		{ "data/character/reimu/attackI007.cv2", 0xae71614f9fc6c7e2 },
		{ "data/character/reimu/attackJ000.cv2", 0x5406630a6a2c483c },
		{ "data/character/reimu/attackJ009.cv2", 0x774594eabd228358 },
		{ "data/character/reimu/attackK001.cv2", 0x35235e300016c1a5 },
		{ "data/character/reimu/attackK002.cv2", 0x60aec25c2f79372d },
		{ "data/character/reimu/attackK003.cv2", 0xa98d3b2594ef7c5a },
		{ "data/character/reimu/attackK004.cv2", 0x92906c11bebb416b },
		{ "data/character/reimu/attackK005.cv2", 0xff81810292ec4618 },
		{ "data/character/reimu/attackK006.cv2", 0xce8faba2f86529de },
		{ "data/character/reimu/attackK007.cv2", 0x19b5f51cb76908ed },
		{ "data/character/reimu/attackL005.cv2", 0x49612687edf61115 },
		{ "data/character/reimu/attackM004.cv2", 0xeeb0a128778f8ae0 },
		{ "data/character/reimu/attackM005.cv2", 0xc8a0280eb569b727 },
		{ "data/character/reimu/attackM006.cv2", 0x39e9082c5193706c },
		{ "data/character/reimu/attackM007.cv2", 0xc36d22de2704c12 },
		{ "data/character/reimu/attackM008.cv2", 0x3ebdd867a00dedeb },
		{ "data/character/reimu/crash002.cv2", 0x50e2c99175b925d3 },
		{ "data/character/reimu/guardBUnder000.cv2", 0xc2fad7e93d6ec9d4 },
		{ "data/character/reimu/guardBUnder001.cv2", 0x6a36375495f7d0c4 },
		{ "data/character/reimu/guardBUpper000.cv2", 0x6619746d35efa4ad },
		{ "data/character/reimu/guardBUpper001.cv2", 0xbe47a13b52bd95a7 },
		{ "data/character/reimu/hitAir000.cv2", 0xe641714be61625d5 },
		{ "data/character/reimu/hitAir002.cv2", 0x4ac82687eb751fc1 },
		{ "data/character/reimu/hitSpin001.cv2", 0xd162bb1cead6c97d },
		{ "data/character/reimu/hitSpin004.cv2", 0xf78dffefc9c6ba24 },
		{ "data/character/reimu/hitUpper001 .cv2", 0x6e9d8a5b09a9e7ef },
		{ "data/character/reimu/jump003 .cv2", 0x755c28486750b5c9 },
		{ "data/character/reimu/jump004 .cv2", 0x2e8692d1c78d47b7 },
		{ "data/character/reimu/jump005 .cv2", 0x2ed9cd8541dc39 },
		{ "data/character/reimu/jump006 .cv2", 0x1ae095641eb2d2cc },
		{ "data/character/reimu/jump007 .cv2", 0x277da71daeccf46e },
		{ "data/character/reimu/jump008 .cv2", 0x67bc65ab47d3d1d4 },
		{ "data/character/reimu/jumpFront001 .cv2", 0x546be03a4c20c0b6 },
		{ "data/character/reimu/jumpFront003 .cv2", 0x2f4f89a8e5a1dae9 },
		{ "data/character/reimu/oharaibou000.cv2", 0xac15066f530169d3 },
		{ "data/character/reimu/oharaibou003.cv2", 0x8d01f7f12862fa1 },
		{ "data/character/reimu/oharaibou004.cv2", 0x528fea4d930021e8 },
		{ "data/character/reimu/palette003.pal", 0x16c145612b568354 },
		{ "data/character/reimu/palette004.pal", 0x85b2823ce4f3936a },
		{ "data/character/reimu/palette007.pal", 0x185a14d2e7411978 },
		{ "data/character/reimu/reimu.pat", 0xbfed8a96794581d9 },
		{ "data/character/reimu/shotA008.cv2", 0xc0ff5ce99c6d1a34 },
		{ "data/character/reimu/shotAb002.cv2", 0x875cb8bdd4777b96 },
		{ "data/character/reimu/shotB000.cv2", 0x98c04aef5d0782ce },
		{ "data/character/reimu/shotB005.cv2", 0x709fcdf6c789a102 },
		{ "data/character/reimu/shotB006.cv2", 0xfb7f02113e8abf25 },
		{ "data/character/reimu/shotBb000.cv2", 0xe9cd231f835030e4 },
		{ "data/character/reimu/shotBb001.cv2", 0x88f6dd2c3a6f8a7a },
		{ "data/character/reimu/shotBc000.cv2", 0xe7c9b54d5f051dec },
		{ "data/character/reimu/shotBc001.cv2", 0xf2c789bb0c40c11f },
		{ "data/character/reimu/shotBc002.cv2", 0x3bf7e3cd2a2ddc3c },
		{ "data/character/reimu/shotCc000.cv2", 0x68fb020495c1a687 },
		{ "data/character/reimu/shotD000.cv2", 0xbb1687b87d516e5f },
		{ "data/character/reimu/shotD001.cv2", 0x862d673e97b7f76 },
		{ "data/character/reimu/shotD002.cv2", 0x84048a20f5f18278 },
		{ "data/character/reimu/spellC006.cv2", 0xfe4c0f1ec77097e7 },
		{ "data/character/reimu/spellC007.cv2", 0x5940a1deec1bbb67 },
		{ "data/character/reimu/spellC008.cv2", 0xa9a777b9df8f0c24 },
		{ "data/character/reimu/spellC009.cv2", 0xd511926ceb898dc6 },
		{ "data/character/reimu/spellC010.cv2", 0xdece4b5b3a9fac7a },
		{ "data/character/reimu/spellCall008.cv2", 0xc04739e00878701f },
		{ "data/character/reimu/spellD006.cv2", 0xf1d925044ee2d325 },
		{ "data/character/reimu/spellDb006.cv2", 0x7baaf9b682188133 },
		{ "data/character/reimu/spellFa002.cv2", 0x4badb87622a985c8 },
		{ "data/character/reimu/spellFa003.cv2", 0xec47a39883008b53 },
		{ "data/character/reimu/spellFa004.cv2", 0xdb9f7433914bc910 },
		{ "data/character/reimu/spellFa005.cv2", 0x50c3a433d67e02a0 },
		{ "data/character/reimu/spellFa006.cv2", 0x78156ac4a0ef984b },
		{ "data/character/reimu/spellFa007.cv2", 0x7b29418636416606 },
		{ "data/character/reimu/spellFa012.cv2", 0x9df6be9acd622164 },
		{ "data/character/remilia/attackAa000.cv2", 0x5f4ada31310f39c3 },
		{ "data/character/remilia/attackAb000.cv2", 0xb04ea3560cb509e0 },
		{ "data/character/remilia/attackAd007.cv2", 0xe94715192be71b5c },
		{ "data/character/remilia/attackBb008.cv2", 0x5a3b79d657bbd4d7 },
		{ "data/character/remilia/attackBb010.cv2", 0x331fa8f5a2985624 },
		{ "data/character/remilia/attackCa000.cv2", 0x19fb7d16c245ed1e },
		{ "data/character/remilia/attackCa001.cv2", 0x76cd6fde101fc6bd },
		{ "data/character/remilia/dashBackAir000.cv2", 0xe2d655b4156b8920 },
		{ "data/character/remilia/dashBackAir007.cv2", 0xfb6a194c34896aae },
		{ "data/character/remilia/guardAir000.cv2", 0xb8493f53d55fcbec },
		{ "data/character/remilia/guardAir001.cv2", 0x89cd5c79e5b03350 },
		{ "data/character/remilia/guardAir002.cv2", 0x29dd35809f845d7a },
		{ "data/character/remilia/guardUpper001.cv2", 0xc8e012afe8718b3f },
		{ "data/character/remilia/hitSpin002.cv2", 0xa33508892de30b55 },
		{ "data/character/remilia/hitSpin005.cv2", 0xc163b7f1a61167e1 },
		{ "data/character/remilia/jump009.cv2", 0x64a70b040e4d1d06 },
		{ "data/character/remilia/jump010.cv2", 0x662e8b125fdeced5 },
		{ "data/character/remilia/remilia.pat", 0x4cbf1090a8bc61e7 },
		{ "data/character/remilia/shotAb000.cv2", 0x869a5650bf86d5ff },
		{ "data/character/remilia/shotAc006.cv2", 0x7df2a5394706f23f },
		{ "data/character/remilia/sit006.cv2", 0x3b41c0949f5072c9 },
		{ "data/character/remilia/spellCa000.cv2", 0x24dc82a1800bb5e2 },
		{ "data/character/remilia/spellCa006.cv2", 0x517f454816306ea1 },
		{ "data/character/remilia/spellCa007.cv2", 0xcb20a8e4d8fc01b5 },
		{ "data/character/remilia/spellDa008.cv2", 0x7c65aa4bcc3dd4a0 },
		{ "data/character/remilia/spellDa009.cv2", 0xefc35e2862b599ef },
		{ "data/character/remilia/spellDb003.cv2", 0xa9f941b3814eb5fe },
		{ "data/character/remilia/spellDb004.cv2", 0x3590d98637dcdf7e },
		{ "data/character/remilia/spellDb005.cv2", 0xff3e5b3732693ab2 },
		{ "data/character/remilia/spellEa005.cv2", 0xed3420abf8b325e1 },
		{ "data/character/remilia/spellFa004.cv2", 0x5292a7f5f659d905 },
		{ "data/character/remilia/spellFa007.cv2", 0x731bc515b2806925 },
		{ "data/character/remilia/spellGa027.cv2", 0x3acc668bd69bf69 },
		{ "data/character/remilia/standUp004.cv2", 0x8af8acd2e108bafa },
		{ "data/character/remilia/walkFront001.cv2", 0xcb517ba7f0e3a5ed },
		{ "data/character/remilia/walkFront003.cv2", 0x8d85a58c7b8e605b },
		{ "data/character/remilia/walkFront005.cv2", 0x3f33bd15584eee15 },
		{ "data/character/remilia/walkFront006.cv2", 0x59f0316f2d5dd283 },
		{ "data/character/remilia/walkFront007.cv2", 0x13b644f216b6046d },
		{ "data/character/sakuya/attackAa000.cv2", 0x77f5c221d7c98708 },
		{ "data/character/sakuya/attackAa004.cv2", 0xa271e73723ae801d },
		{ "data/character/sakuya/attackAb002.cv2", 0x7b65fce24a1d3f7d },
		{ "data/character/sakuya/attackAb007.cv2", 0x1a34fcf32642fe1f },
		{ "data/character/sakuya/attackAb008.cv2", 0x58a64824fab7cd3 },
		{ "data/character/sakuya/attackCa003.cv2", 0x785b6190bf5d7710 },
		{ "data/character/sakuya/attackCd000.cv2", 0xa63b9c3a57783d9d },
		{ "data/character/sakuya/attackCd001.cv2", 0xa95e0df3d27f594 },
		{ "data/character/sakuya/dashFront003.cv2", 0x4c92ec66748bc7ac },
		{ "data/character/sakuya/down000.cv2", 0x8b0be3ba2c4b2d36 },
		{ "data/character/sakuya/down001.cv2", 0x8e88c75991e4ff69 },
		{ "data/character/sakuya/down003.cv2", 0xea457500bcc28fca },
		{ "data/character/sakuya/down004.cv2", 0x41ced982023df440 },
		{ "data/character/sakuya/down005.cv2", 0xf08f42c549cad872 },
		{ "data/character/sakuya/down006.cv2", 0x57c8a6ae9b5110c9 },
		{ "data/character/sakuya/down007.cv2", 0xc9221006846af27e },
		{ "data/character/sakuya/guardUnder000.cv2", 0xbfb8c4286246c21b },
		{ "data/character/sakuya/guardUnder001.cv2", 0x7ee8ea57a0ed2305 },
		{ "data/character/sakuya/guardUnder002.cv2", 0x294539ed2505906b },
		{ "data/character/sakuya/guardUpper000.cv2", 0x980addf7692224f4 },
		{ "data/character/sakuya/guardUpper001.cv2", 0x81f7ba3cc75d717e },
		{ "data/character/sakuya/hitAir000.cv2", 0x1ff7391f0b1465e8 },
		{ "data/character/sakuya/hitAir001.cv2", 0x9ebdd7669b4c9722 },
		{ "data/character/sakuya/hitAir003.cv2", 0x762782380b8b990d },
		{ "data/character/sakuya/hitAir004.cv2", 0x7203d9f6e8cff79b },
		{ "data/character/sakuya/hitAir005.cv2", 0x3a963e7e91b0ab3e },
		{ "data/character/sakuya/hitAir006.cv2", 0xe53fc382acaeb881 },
		{ "data/character/sakuya/hitSit002.cv2", 0xd77088c86858b24d },
		{ "data/character/sakuya/jump000.cv2", 0x4e81f176b1857c8f },
		{ "data/character/sakuya/jump001.cv2", 0xcd4a598bde815a9a },
		{ "data/character/sakuya/jump002.cv2", 0x9cd13de30540b356 },
		{ "data/character/sakuya/sakuya.pat", 0x61dc95d3e40c087e },
		{ "data/character/sakuya/shotAa000.cv2", 0x858281c2755bad4c },
		{ "data/character/sakuya/shotAa001.cv2", 0x37a4b58a227e544b },
		{ "data/character/sakuya/shotAb008.cv2", 0x445f9655d50fe15b },
		{ "data/character/sakuya/shotAc000.cv2", 0xd22283eb66d9342 },
		{ "data/character/sakuya/shotBa005.cv2", 0x19403b5f04566e3c },
		{ "data/character/sakuya/shotBa006.cv2", 0x97cdc288251ac6 },
		{ "data/character/sakuya/shotBa007.cv2", 0x626e4b2efe0646f0 },
		{ "data/character/sakuya/shotBa008.cv2", 0x30afcf4acde1f5ac },
		{ "data/character/sakuya/spellAa011.cv2", 0xf72e386d5de54502 },
		{ "data/character/sakuya/spellBa011.cv2", 0x953aa651168ab71c },
		{ "data/character/sakuya/spellCa005.cv2", 0xbd7a1d36a2d4e63d },
		{ "data/character/sakuya/spellDa009.cv2", 0x4583d1736e2198ca },
		{ "data/character/sakuya/spellFa012.cv2", 0xf44c20468fddbbf3 },
		{ "data/character/sakuya/spellFa013.cv2", 0xa400112ef21ffd93 },
		{ "data/character/sakuya/spellGa004.cv2", 0x8a0d1d1ca014f6d5 },
		{ "data/character/sanae/attackAa002.cv2", 0x8249c4bc0d9c9ae9 },
		{ "data/character/sanae/attackAa003.cv2", 0xb19fd276655677cd },
		{ "data/character/sanae/attackBa000.cv2", 0x8e0ea61390b8c115 },
		{ "data/character/sanae/attackBa001.cv2", 0x1289f52a0b620d2e },
		{ "data/character/sanae/attackBa002.cv2", 0x3388d7686e38ec20 },
		{ "data/character/sanae/attackBa003.cv2", 0xb6c48fdf53521dd },
		{ "data/character/sanae/attackBa004.cv2", 0xdc78b9a8fa6b160e },
		{ "data/character/sanae/attackBa005.cv2", 0xdd4c48a40b5c41e2 },
		{ "data/character/sanae/attackBa006.cv2", 0x888eef63ba649fa8 },
		{ "data/character/sanae/attackBa007.cv2", 0x52164bd23606ad4d },
		{ "data/character/sanae/attackBa008.cv2", 0xc9ab84cca0a3388c },
		{ "data/character/sanae/attackBc009.cv2", 0xe6f0d2b137e88111 },
		{ "data/character/sanae/attackCa000.cv2", 0xca6985e504e7ac99 },
		{ "data/character/sanae/attackCa001.cv2", 0x865b64ebd93c6561 },
		{ "data/character/sanae/attackCa002.cv2", 0x674a4f9a4ac63571 },
		{ "data/character/sanae/attackCa003.cv2", 0xe40aef5cf6cbddd2 },
		{ "data/character/sanae/attackCa004.cv2", 0x3afdc99398e76bcb },
		{ "data/character/sanae/attackCa005.cv2", 0xb90f377d22dc14c6 },
		{ "data/character/sanae/attackCa006.cv2", 0x82ff4a9f3785edd1 },
		{ "data/character/sanae/attackCa007.cv2", 0x2f0207880d85dcb9 },
		{ "data/character/sanae/attackCa008.cv2", 0x974e8140e01fce83 },
		{ "data/character/sanae/attackCa009.cv2", 0xd8c686cc07797361 },
		{ "data/character/sanae/attackCd007.cv2", 0x358969ccb7a69c1c },
		{ "data/character/sanae/attackCd008.cv2", 0x6a5f3ab59f690622 },
		{ "data/character/sanae/attackCe000.cv2", 0x5a0b8cdfeb0d62d8 },
		{ "data/character/sanae/attackCe001.cv2", 0x55d70e1c7757678c },
		{ "data/character/sanae/attackCe002.cv2", 0x9647284f06d859e4 },
		{ "data/character/sanae/attackCe003.cv2", 0x85891189f8bb2ef6 },
		{ "data/character/sanae/attackCe004.cv2", 0xb9387b579b174afc },
		{ "data/character/sanae/attackCe005.cv2", 0x1e12a3085411459d },
		{ "data/character/sanae/attackCe006.cv2", 0x4a441435bf715e0a },
		{ "data/character/sanae/attackCe007.cv2", 0xc6eac597bad2e15a },
		{ "data/character/sanae/attackCe008.cv2", 0x766519416d711abb },
		{ "data/character/sanae/attackCe009.cv2", 0xd6818ab62d523d10 },
		{ "data/character/sanae/attackCe010.cv2", 0x993e8929cb07ea03 },
		{ "data/character/sanae/attackCe011.cv2", 0x7dd262826ff0db18 },
		{ "data/character/sanae/attackCe012.cv2", 0x4459b6a210ea8607 },
		{ "data/character/sanae/attackCe013.cv2", 0x832e2a7513af6f57 },
		{ "data/character/sanae/attackCe014.cv2", 0x56e89ce6d2ffd54 },
		{ "data/character/sanae/attackCe015.cv2", 0xcf4f0a047e08de05 },
		{ "data/character/sanae/attackCf000.cv2", 0x3504db1f1d3f4076 },
		{ "data/character/sanae/attackCf001.cv2", 0xba81c168d88cb76d },
		{ "data/character/sanae/attackCf002.cv2", 0xbd34ec2e4c6621bf },
		{ "data/character/sanae/attackCf003.cv2", 0x6a38fa124c266dbd },
		{ "data/character/sanae/attackCf004.cv2", 0xb879055be33726f2 },
		{ "data/character/sanae/attackCf005.cv2", 0xbc804260aa8d5bdd },
		{ "data/character/sanae/attackCf006.cv2", 0xb7b7c8eb8619d5b4 },
		{ "data/character/sanae/attackCf007.cv2", 0x3524f4fcde4306ce },
		{ "data/character/sanae/attackCf008.cv2", 0xcc542e01c6f290dd },
		{ "data/character/sanae/attackCf009.cv2", 0x8223e998105b0782 },
		{ "data/character/sanae/attackCg000.cv2", 0x90d6c0af4ed1673e },
		{ "data/character/sanae/attackCg001.cv2", 0x9e40228f73e4306a },
		{ "data/character/sanae/attackCg002.cv2", 0x8b022073b55bee71 },
		{ "data/character/sanae/attackCg003.cv2", 0x129bf6b89a617c00 },
		{ "data/character/sanae/attackCg004.cv2", 0x7cc3c80524bf49c0 },
		{ "data/character/sanae/attackCg005.cv2", 0x923631b9f80616b7 },
		{ "data/character/sanae/attackCg006.cv2", 0x581ef8324b728999 },
		{ "data/character/sanae/attackCg007.cv2", 0xdf71bca734244518 },
		{ "data/character/sanae/attackCg008.cv2", 0x408467f8a8a60647 },
		{ "data/character/sanae/back/spell000.cv2", 0xd5dfabbf209ba421 },
		{ "data/character/sanae/bulletEb002.cv2", 0x4063ddca1d83b950 },
		{ "data/character/sanae/bulletEc000.cv2", 0xd69d3e41cfee8462 },
		{ "data/character/sanae/bulletEc001.cv2", 0x6100ff07481ce496 },
		{ "data/character/sanae/bulletEc002.cv2", 0xbca7165ffaccc7cc },
		{ "data/character/sanae/bulletEc003.cv2", 0x91b296f5fa3dd27e },
		{ "data/character/sanae/bulletEc004.cv2", 0xc6c62e0cda35f6c0 },
		{ "data/character/sanae/bulletEc005.cv2", 0x69bb0b2f9824aab8 },
		{ "data/character/sanae/hitSpin003.cv2", 0x7a59d7d8b2d0f275 },
		{ "data/character/sanae/opSuwakoDa000.cv2", 0x49ccd0450335c335 },
		{ "data/character/sanae/opSuwakoDa001.cv2", 0x3c9eeea9862f5f1d },
		{ "data/character/sanae/opSuwakoDa002.cv2", 0x26f2d79d4c774bfe },
		{ "data/character/sanae/opSuwakoDa003.cv2", 0xfbab1fd94de48414 },
		{ "data/character/sanae/opSuwakoDa004.cv2", 0xe4d45116b3cdef81 },
		{ "data/character/sanae/opSuwakoDa005.cv2", 0x33357d86a692318d },
		{ "data/character/sanae/opSuwakoDa006.cv2", 0x4d9f59565a74ef92 },
		{ "data/character/sanae/opSuwakoDa007.cv2", 0x62b4c9dd1b6a0dbf },
		{ "data/character/sanae/palette001.pal", 0x3938ba16fee9c130 },
		{ "data/character/sanae/palette004.pal", 0xc3b74d9631d30e4c },
		{ "data/character/sanae/palette006.pal", 0x6ff2e67b8347eb28 },
		{ "data/character/sanae/palette007.pal", 0xbe0a21ab8ec6d946 },
		{ "data/character/sanae/sanae.pat", 0xb858a5c7c3a4849 },
		{ "data/character/sanae/spellCa006.cv2", 0xba105ff8872de6e5 },
		{ "data/character/sanae/spellCa007.cv2", 0xc17fd3b286f37a77 },
		{ "data/character/sanae/stand005.cv2", 0xe47df9761eaf667b },
		{ "data/character/suika/attackCa000.cv2", 0x6269ae0e6cd116a5 },
		{ "data/character/suika/attackCa001.cv2", 0xc4d9d0a89885cc23 },
		{ "data/character/suika/attackCa003.cv2", 0x25624b5570f36e80 },
		{ "data/character/suika/attackCa004.cv2", 0x30f52af6ab9d5340 },
		{ "data/character/suika/attackCa005.cv2", 0x7f9ed4dea54da8e8 },
		{ "data/character/suika/attackCa006.cv2", 0x95d7fda8728f5cd9 },
		{ "data/character/suika/attackCa009.cv2", 0x371419273336aa24 },
		{ "data/character/suika/attackCb012.cv2", 0xb064aa206f234381 },
		{ "data/character/suika/attackCb013.cv2", 0xd4fdf117e987c2ba },
		{ "data/character/suika/attackCd004.cv2", 0xfb9470d223e90fb5 },
		{ "data/character/suika/attackCd009.cv2", 0xdb82342d35ef8828 },
		{ "data/character/suika/down007.cv2", 0xbaeb50083f47a72c },
		{ "data/character/suika/down008.cv2", 0xd8d2f2efe46713db },
		{ "data/character/suika/down009.cv2", 0x1a0c7ddf5501778c },
		{ "data/character/suika/jumpBack002.cv2", 0xb0e3c6554308c2a5 },
		{ "data/character/suika/shotAa000.cv2", 0xad7c9bc0e6dfcd7a },
		{ "data/character/suika/shotAc000.cv2", 0xf0785a2e1077e41b },
		{ "data/character/suika/shotBa008.cv2", 0x6a548b09f4818ef2 },
		{ "data/character/suika/shotBa010.cv2", 0x64226af8c5170f5c },
		{ "data/character/suika/spellAa002.cv2", 0xf7f77915fa8640af },
		{ "data/character/suika/spellAa003.cv2", 0x814858add1d7e55d },
		{ "data/character/suika/spellAa005.cv2", 0xa5424ff26b296226 },
		{ "data/character/suika/spellBa013.cv2", 0xa03dd31372e118f6 },
		{ "data/character/suika/spellBa014.cv2", 0x3a6b803894d0b48f },
		{ "data/character/suika/spellCa000.cv2", 0x181ed5c64a34cefe },
		{ "data/character/suika/spellCa001.cv2", 0x59bf58fe67b088cd },
		{ "data/character/suika/spellCa003.cv2", 0x4f84375e56883e76 },
		{ "data/character/suika/spellCa004.cv2", 0x329c7eb4fd3574e1 },
		{ "data/character/suika/spellGa004.cv2", 0x10cbb864a93d884d },
		{ "data/character/suika/spellGa010.cv2", 0x26a0dff270cba1b1 },
		{ "data/character/suika/spellGa012.cv2", 0xe7a79d2fa07fc660 },
		{ "data/character/suika/spellHa000.cv2", 0x862b35f7e7e3cda8 },
		{ "data/character/suika/spellHa005.cv2", 0xd3af821667486941 },
		{ "data/character/suika/spellHa006.cv2", 0xcd10e9ab7aa4ab25 },
		{ "data/character/suika/spellHa007.cv2", 0x9a99264588b39f87 },
		{ "data/character/suika/spellHa008.cv2", 0x9cca533777b2ce5e },
		{ "data/character/suika/spellIa000.cv2", 0xf8203d22d8ddaec2 },
		{ "data/character/suika/spellIa002.cv2", 0x854abf7f95ee9af7 },
		{ "data/character/suika/spellIa003.cv2", 0x27255537ce64fe11 },
		{ "data/character/suika/spellIa004.cv2", 0xc4d3822d2ef5cd83 },
		{ "data/character/suika/spellIa005.cv2", 0x231b8967f94d6482 },
		{ "data/character/suika/stand000.cv2", 0xce4fdb3f08c89b01 },
		{ "data/character/suika/stand011.cv2", 0xb5ad2eab5d0d9153 },
		{ "data/character/suika/suika.pat", 0x9c1401856ad54595 },
		{ "data/character/suika/walkBack003.cv2", 0x486451c3ba179b37 },
		{ "data/character/suika/walkFront003.cv2", 0x7870472b7a1860b9 },
		{ "data/character/suwako/attackAa005.cv2", 0x9519826c6aeefef },
		{ "data/character/suwako/attackCc012.cv2", 0x3ff8d9913c5fc592 },
		{ "data/character/suwako/back/spell000.cv2", 0x8b16370aef4f873b },
		{ "data/character/suwako/objectFc000.cv2", 0xf1f1346cb39e2893 },
		{ "data/character/suwako/objectFc001.cv2", 0x38cccdde79cd7231 },
		{ "data/character/suwako/objectFc002.cv2", 0xa3cd997175960bf9 },
		{ "data/character/suwako/objectFc003.cv2", 0x88d7a6824ef31fa6 },
		{ "data/character/suwako/palette006.pal", 0x2693bae35318b1e2 },
		{ "data/character/suwako/suwako.pat", 0x6184477427ff9d1f },
		{ "data/character/tenshi/attackDa000.cv2", 0x80f852a6d84f3f51 },
		{ "data/character/tenshi/attackDa004.cv2", 0xb1f16b045adc42df },
		{ "data/character/tenshi/attackDa005.cv2", 0x9e8cddab0e4467af },
		{ "data/character/tenshi/attackDa006.cv2", 0xf18d2b8fac92fbe1 },
		{ "data/character/tenshi/attackDc000.cv2", 0x6d67d44ebab60e7f },
		{ "data/character/tenshi/attackDc001.cv2", 0xa45f75b8353d9dca },
		{ "data/character/tenshi/attackDc002.cv2", 0x176003faecf1045c },
		{ "data/character/tenshi/attackDc003.cv2", 0x8dfddb1275750282 },
		{ "data/character/tenshi/attackDc004.cv2", 0x7e284c7614234e1c },
		{ "data/character/tenshi/attackDc005.cv2", 0x5b6d03ea76273d7 },
		{ "data/character/tenshi/attackDc006.cv2", 0xdedd5d11010735c8 },
		{ "data/character/tenshi/attackDc007.cv2", 0x4153b1001521e0b2 },
		{ "data/character/tenshi/attackDc008.cv2", 0xc69831a07bf9e05 },
		{ "data/character/tenshi/dashAirFront000.cv2", 0x78f8fadba5cc7850 },
		{ "data/character/tenshi/guardAir000.cv2", 0x9a60addb30707279 },
		{ "data/character/tenshi/guardAir001.cv2", 0xaaca8b7027624f68 },
		{ "data/character/tenshi/guardAir002.cv2", 0x5bcc055cfe8e8dda },
		{ "data/character/tenshi/guardAir003.cv2", 0xd68b6851f7f1f995 },
		{ "data/character/tenshi/guardAir004.cv2", 0x95c95da582655844 },
		{ "data/character/tenshi/guardSit000.cv2", 0xa774ea8324613210 },
		{ "data/character/tenshi/guardSit001.cv2", 0xb0eb3a56e7b12588 },
		{ "data/character/tenshi/guardSit002.cv2", 0x4cb6e0faa17f77d },
		{ "data/character/tenshi/guardSit003.cv2", 0xab6a01663a9e273d },
		{ "data/character/tenshi/guardSit004.cv2", 0xa774ea8324613210 },
		{ "data/character/tenshi/guardSitB000.cv2", 0x63f6f776d8bf0abf },
		{ "data/character/tenshi/guardSitB001.cv2", 0x1141293fc9ea407c },
		{ "data/character/tenshi/guardSitB002.cv2", 0x95d42078658a3853 },
		{ "data/character/tenshi/guardSitB003.cv2", 0xba6251039dd06ac6 },
		{ "data/character/tenshi/guardSitB004.cv2", 0xda3dcbf402c6c45 },
		{ "data/character/tenshi/guardUpper000.cv2", 0x6351b1801e218af0 },
		{ "data/character/tenshi/guardUpper001.cv2", 0x5bcc374a959cf0f8 },
		{ "data/character/tenshi/guardUpper002.cv2", 0x2ff9f6bb2746403f },
		{ "data/character/tenshi/guardUpper003.cv2", 0x7be14423fbcc54f },
		{ "data/character/tenshi/guardUpper004.cv2", 0x6351b1801e218af0 },
		{ "data/character/tenshi/guardUpperB001.cv2", 0x1ecf0422ff308fc4 },
		{ "data/character/tenshi/guardUpperB002.cv2", 0x6ebc9013bd7681a6 },
		{ "data/character/tenshi/guardUpperB003.cv2", 0x58f84e259bdcaeb5 },
		{ "data/character/tenshi/hitSpin000.cv2", 0x3274c584e8671cb0 },
		{ "data/character/tenshi/hitSpin003.cv2", 0x81dad7986e010558 },
		{ "data/character/tenshi/jump000.cv2", 0xcf35bc3b0445d10 },
		{ "data/character/tenshi/jump001.cv2", 0x5494d26a9851bf5d },
		{ "data/character/tenshi/jump002.cv2", 0x27e5a5ca69e14b0a },
		{ "data/character/tenshi/jump003.cv2", 0x5891ed6cee31a6c6 },
		{ "data/character/tenshi/jump004.cv2", 0x6dde2c06f4e059dc },
		{ "data/character/tenshi/jump005.cv2", 0xc4f8e637a9e848db },
		{ "data/character/tenshi/jump006.cv2", 0xf910cee358bfe3c },
		{ "data/character/tenshi/jump007.cv2", 0x7553f498aef14e40 },
		{ "data/character/tenshi/jump008.cv2", 0xa1965921ab5a438f },
		{ "data/character/tenshi/jump009.cv2", 0x2bd421424b9fb579 },
		{ "data/character/tenshi/shotBb001.cv2", 0x228e3d09b12d035e },
		{ "data/character/tenshi/shotBb002.cv2", 0x59e1b535f5d4cc8f },
		{ "data/character/tenshi/shotBb003.cv2", 0x9a452a4a662af50c },
		{ "data/character/tenshi/shotBb004.cv2", 0xe301764fd4f78d2a },
		{ "data/character/tenshi/shotBb005.cv2", 0x1cec266507d1dab2 },
		{ "data/character/tenshi/shotBb006.cv2", 0xff5e35c75b0a656c },
		{ "data/character/tenshi/shotBb007.cv2", 0x9c3d0b27b137133a },
		{ "data/character/tenshi/shotBb008.cv2", 0x83323e9488c23b88 },
		{ "data/character/tenshi/shotBb009.cv2", 0x47da28fca2fc2883 },
		{ "data/character/tenshi/shotBb010.cv2", 0x81229d66e41d7369 },
		{ "data/character/tenshi/shotBb011.cv2", 0x4008a2416e09d666 },
		{ "data/character/tenshi/shotBc000.cv2", 0x892f0aca49b5074e },
		{ "data/character/tenshi/shotBc001.cv2", 0x8382cadf10d22a93 },
		{ "data/character/tenshi/shotBc002.cv2", 0x787fa11884ea1a1b },
		{ "data/character/tenshi/shotBc003.cv2", 0xb3f9f44d947d0ed3 },
		{ "data/character/tenshi/shotCc008.cv2", 0xbdf203573f21e039 },
		{ "data/character/tenshi/shotCc009.cv2", 0x6c60f55687581e43 },
		{ "data/character/tenshi/sit000.cv2", 0x177b59963d884c27 },
		{ "data/character/tenshi/sit001.cv2", 0xb023b8f721c6d8 },
		{ "data/character/tenshi/sit002.cv2", 0xdb8514e8dd9ee12c },
		{ "data/character/tenshi/sit003.cv2", 0x79c3ab815850981f },
		{ "data/character/tenshi/sit004.cv2", 0xf43faed5fb6884ac },
		{ "data/character/tenshi/sit005.cv2", 0x4695ad34e0fcf953 },
		{ "data/character/tenshi/sit006.cv2", 0xcb73d27723e08017 },
		{ "data/character/tenshi/sit007.cv2", 0x45123c1275ee2876 },
		{ "data/character/tenshi/sit008.cv2", 0x2c86369984b0ac75 },
		{ "data/character/tenshi/sit009.cv2", 0x9fe6581df13bc12e },
		{ "data/character/tenshi/sit010.cv2", 0x7084b68bfe9776f6 },
		{ "data/character/tenshi/sit011.cv2", 0xdd6498fc20444515 },
		{ "data/character/tenshi/spellMa004.cv2", 0x81e91704bc69da29 },
		{ "data/character/tenshi/spellMa005.cv2", 0xe68168fcfb2625f2 },
		{ "data/character/tenshi/spellMa011.cv2", 0xe4f512a1b0529480 },
		{ "data/character/tenshi/spellMa012.cv2", 0x59d85ce6d44d3c27 },
		{ "data/character/tenshi/spellMa013.cv2", 0x1cc49ddb30c39c25 },
		{ "data/character/tenshi/standUp004.cv2", 0xf168a0b6cf6a78a1 },
		{ "data/character/tenshi/standUp005.cv2", 0x98a5544b4af23a38 },
		{ "data/character/tenshi/tenshi.pat", 0xda0192010d3bff8e },
		{ "data/character/tenshi/walkFront000.cv2", 0xe8a482ae212e82cf },
		{ "data/character/tenshi/walkFront001.cv2", 0x926896927fe2945b },
		{ "data/character/tenshi/walkFront002.cv2", 0x41ccfa7818b3c90f },
		{ "data/character/tenshi/walkFront003.cv2", 0x9814a5d8732ee271 },
		{ "data/character/tenshi/walkFront004.cv2", 0xd17540a972378278 },
		{ "data/character/tenshi/walkFront005.cv2", 0x336664d71ae34d87 },
		{ "data/character/tenshi/walkFront006.cv2", 0x13f43f26e9f1e3ee },
		{ "data/character/tenshi/walkFront007.cv2", 0x1b4c235db0b1c160 },
		{ "data/character/udonge/attackAe001.cv2", 0xef03448127656c94 },
		{ "data/character/udonge/attackBa002.cv2", 0x2b73d75ac6c949cb },
		{ "data/character/udonge/attackBa003.cv2", 0x7787917e4e16875c },
		{ "data/character/udonge/attackCb000.cv2", 0x937a19b94cae06d4 },
		{ "data/character/udonge/attackCb001.cv2", 0x2c78d27eed919bd },
		{ "data/character/udonge/attackCb002.cv2", 0x5a69d57baa5ab3c0 },
		{ "data/character/udonge/attackCb003.cv2", 0xc39f720165b893c0 },
		{ "data/character/udonge/attackCb004.cv2", 0xc225ae4c8c085f6b },
		{ "data/character/udonge/attackCb005.cv2", 0xb509cacdb9005b60 },
		{ "data/character/udonge/attackCb006.cv2", 0x8021c5228e129fb8 },
		{ "data/character/udonge/attackCb007.cv2", 0x6715b390ba7dc126 },
		{ "data/character/udonge/attackCb008.cv2", 0x7bb4dbb92a339b47 },
		{ "data/character/udonge/attackCb009.cv2", 0xe66df62bfeb445dd },
		{ "data/character/udonge/attackCb010.cv2", 0xb44679c2f80a3310 },
		{ "data/character/udonge/crash000.cv2", 0xc1372a36c533f014 },
		{ "data/character/udonge/crash001.cv2", 0x5015ba80e372a359 },
		{ "data/character/udonge/guardUpperB001.cv2", 0xe8d0c15af8e45527 },
		{ "data/character/udonge/jump000.cv2", 0x78acc34ac0a913d9 },
		{ "data/character/udonge/jump001.cv2", 0x56a99306917c0ab6 },
		{ "data/character/udonge/jump002.cv2", 0xcf48ef351b6f9214 },
		{ "data/character/udonge/jump003.cv2", 0x45425ff6eecea769 },
		{ "data/character/udonge/jump004.cv2", 0x954d45e132af26f },
		{ "data/character/udonge/jump005.cv2", 0x3c9d61c726edac61 },
		{ "data/character/udonge/jump006.cv2", 0x27b7d1de0846fe84 },
		{ "data/character/udonge/jump007.cv2", 0x7a62ebe974dcb8ab },
		{ "data/character/udonge/jump008.cv2", 0x1c33dcdded3dda3 },
		{ "data/character/udonge/jump009.cv2", 0x6bf864a06ac75df0 },
		{ "data/character/udonge/jump010.cv2", 0x3f264c53781d42b1 },
		{ "data/character/udonge/jump011.cv2", 0xd35afdf58ed799db },
		{ "data/character/udonge/jumpBack000.cv2", 0x90ee0fd2ef2c215e },
		{ "data/character/udonge/jumpBack001.cv2", 0x2145b2583be6106f },
		{ "data/character/udonge/jumpBack002.cv2", 0x61e8beb92f7081c7 },
		{ "data/character/udonge/jumpBack003.cv2", 0x1ec892f7fff770c3 },
		{ "data/character/udonge/jumpBack004.cv2", 0xbf5e10970af3e3db },
		{ "data/character/udonge/jumpFront000.cv2", 0xfcbf95cf466095ea },
		{ "data/character/udonge/jumpFront001.cv2", 0x7b34837a61b96ce1 },
		{ "data/character/udonge/jumpFront002.cv2", 0x95a0b3f263cb1e28 },
		{ "data/character/udonge/jumpFront003.cv2", 0xc6ed3238de557565 },
		{ "data/character/udonge/jumpFront004.cv2", 0x268e20f08aab3599 },
		{ "data/character/udonge/shotBe000.cv2", 0x8863a4ad5a90cbf6 },
		{ "data/character/udonge/shotBe001.cv2", 0x171c0c7c51df2add },
		{ "data/character/udonge/shotBe002.cv2", 0x36f4175cff44a6b6 },
		{ "data/character/udonge/shotBe003.cv2", 0xf5b8578a91ed0bd8 },
		{ "data/character/udonge/shotBe004.cv2", 0xee4505f073cddeab },
		{ "data/character/udonge/shotBe005.cv2", 0x78ba167604cef20b },
		{ "data/character/udonge/shotBe006.cv2", 0x985296a22a6d7d34 },
		{ "data/character/udonge/shotBe007.cv2", 0xbee26d0f6d448956 },
		{ "data/character/udonge/shotBf000.cv2", 0x178fe743ff41a1f2 },
		{ "data/character/udonge/shotBf001.cv2", 0x50069752c3fa25bc },
		{ "data/character/udonge/shotBf002.cv2", 0xa1a4c86c8e1138c4 },
		{ "data/character/udonge/shotBf003.cv2", 0x170417b5aeaa1184 },
		{ "data/character/udonge/shotBg000.cv2", 0x6b6605a8cdc80aa8 },
		{ "data/character/udonge/shotBg001.cv2", 0x3f4aac387f44a28d },
		{ "data/character/udonge/shotBg002.cv2", 0x2d6efae3b486e804 },
		{ "data/character/udonge/shotBg003.cv2", 0x5a661d929a03c7 },
		{ "data/character/udonge/shotBg004.cv2", 0xbd2bcfeba1e0cbe2 },
		{ "data/character/udonge/shotBg005.cv2", 0x32884d6ec8451b8 },
		{ "data/character/udonge/shotBg006.cv2", 0xa661571878cb0854 },
		{ "data/character/udonge/shotBg007.cv2", 0xf5159a9576552671 },
		{ "data/character/udonge/shotBh000.cv2", 0xb7ab3155da143124 },
		{ "data/character/udonge/shotBh001.cv2", 0xf981b356dfbe9b11 },
		{ "data/character/udonge/shotBh002.cv2", 0x8cbf4ae2b9a9e7c },
		{ "data/character/udonge/shotBh003.cv2", 0xf93e08dc8bb68a9f },
		{ "data/character/udonge/shotBh004.cv2", 0xa24c86e631949ecf },
		{ "data/character/udonge/spellAb002.cv2", 0x3ae05f873dae74c7 },
		{ "data/character/udonge/spellAb003.cv2", 0xa0a42eedfd059602 },
		{ "data/character/udonge/spellAb004.cv2", 0x784706f6cfbcc2f5 },
		{ "data/character/udonge/spellAb005.cv2", 0xab4e0c1623b53fba },
		{ "data/character/udonge/spellAb006.cv2", 0xb4eccc7de5090a5 },
		{ "data/character/udonge/spellAb007.cv2", 0x1c23c4457bb190b8 },
		{ "data/character/udonge/spellAb008.cv2", 0x300908b375f16f30 },
		{ "data/character/udonge/spellAb009.cv2", 0xf132fbe7d8d44d30 },
		{ "data/character/udonge/stand000.cv2", 0x6c5e2b2ed525f1d0 },
		{ "data/character/udonge/stand001.cv2", 0x602cea49978e13bb },
		{ "data/character/udonge/stand002.cv2", 0xb488a84b6c917af9 },
		{ "data/character/udonge/stand003.cv2", 0x910c058a87df1926 },
		{ "data/character/udonge/stand004.cv2", 0xc2cfda3f1b811f5e },
		{ "data/character/udonge/stand005.cv2", 0x402bae0965447a5d },
		{ "data/character/udonge/stand006.cv2", 0x2449b2b5b6054032 },
		{ "data/character/udonge/stand007.cv2", 0x1070f61c0404ca95 },
		{ "data/character/udonge/stand008.cv2", 0xd97d57e907484901 },
		{ "data/character/udonge/udonge.pat", 0xd638d8949aa6fc2b },
		{ "data/character/utsuho/back/spell000.cv2", 0xb3958a5a91f30b0d },
		{ "data/character/utsuho/palette000b.pal", 0x32fb37de580d6838 },
		{ "data/character/utsuho/palette001b.pal", 0x8acf82b84b7106f7 },
		{ "data/character/utsuho/palette002b.pal", 0xe8b77ce61590731b },
		{ "data/character/utsuho/palette003b.pal", 0x6c6ad888a86adf60 },
		{ "data/character/utsuho/palette004b.pal", 0x898d1b65951fad6 },
		{ "data/character/utsuho/palette005b.pal", 0x808888f26546f4d1 },
		{ "data/character/utsuho/palette006b.pal", 0xd435bb30c52ca59d },
		{ "data/character/utsuho/palette007b.pal", 0xc8ef0829adca1898 },
		{ "data/character/utsuho/utsuho.pat", 0x4268094a8653984a },
		{ "data/character/youmu/bulletBe000.cv2", 0x8719430b2ca7f21 },
		{ "data/character/youmu/bulletBe001.cv2", 0xaab2a737b0c508db },
		{ "data/character/youmu/bulletBe002.cv2", 0xb6668cdf72f17138 },
		{ "data/character/youmu/bulletBe003.cv2", 0x581ae2a5ef0b81fa },
		{ "data/character/youmu/bulletBe004.cv2", 0xe50ee68aa1dda405 },
		{ "data/character/youmu/jump003.cv2", 0x4aa4ccd684a06511 },
		{ "data/character/youmu/jump005.cv2", 0x956734d8f93133a7 },
		{ "data/character/youmu/jump007.cv2", 0x2fd412ab4ed9c80a },
		{ "data/character/youmu/spellGa001.cv2", 0xf18600e0111e15d0 },
		{ "data/character/youmu/spellHb010.cv2", 0x6d992366b0f5cd24 },
		{ "data/character/youmu/spellIa008.cv2", 0x69b61d495ff013b0 },
		{ "data/character/youmu/spellJa008.cv2", 0xf6d76c871859189 },
		{ "data/character/youmu/youmu.pat", 0xe5224ed74eb0ec63 },
		{ "data/character/yukari/Chen000.cv2", 0x917273e72730dea9 },
		{ "data/character/yukari/attackAb001.cv2", 0x6d6360cdd3aa88d2 },
		{ "data/character/yukari/attackAb002.cv2", 0x1939a9b49940a2a6 },
		{ "data/character/yukari/attackAb003.cv2", 0xfc50414be80fb5b8 },
		{ "data/character/yukari/attackAb004.cv2", 0x9d47d8265de2931a },
		{ "data/character/yukari/attackBb009.cv2", 0x5e08790213ca330d },
		{ "data/character/yukari/bulletDb000.cv2", 0xc3806a88a62433ca },
		{ "data/character/yukari/bulletDc000.cv2", 0xf7bac32a83a0d1e9 },
		{ "data/character/yukari/down006.cv2", 0x197c669ef7d511ed },
		{ "data/character/yukari/spellCall001.cv2", 0x6e70d8d2ba838682 },
		{ "data/character/yukari/spellDa002.cv2", 0x9ce40e5323ffbb1d },
		{ "data/character/yukari/spellEa003.cv2", 0x1988d4a19a6aeb8e },
		{ "data/character/yukari/spellFa001.cv2", 0x6aa89368540c2f94 },
		{ "data/character/yukari/yukari.pat", 0xf763057ac8df70d2 },
		{ "data/character/yuyuko/attackAa000.cv2", 0xe7a788597052673f },
		{ "data/character/yuyuko/attackAa007.cv2", 0xa37815395f9f216a },
		{ "data/character/yuyuko/attackAa008.cv2", 0xa37815395f9f216a },
		{ "data/character/yuyuko/attackAd000.cv2", 0xe285363251e1b7cc },
		{ "data/character/yuyuko/attackAd005.cv2", 0x3525a1dfd6038187 },
		{ "data/character/yuyuko/attackBa002.cv2", 0x1469b4167ee8aa84 },
		{ "data/character/yuyuko/attackCa005.cv2", 0xa44e4a8831f8ea9b },
		{ "data/character/yuyuko/hitSpin001.cv2", 0x5096d43f1539b2f3 },
		{ "data/character/yuyuko/hitSpin004.cv2", 0x5f1e307c650f9fc4 },
		{ "data/character/yuyuko/palette001.pal", 0xc95536e58bca5259 },
		{ "data/character/yuyuko/shotCa010.cv2", 0xd9e34cca5b775a8b },
		{ "data/character/yuyuko/spellAa007.cv2", 0x5f0d8e6cfba061e9 },
		{ "data/character/yuyuko/spellDa003.cv2", 0xbde8a08912d7552f },
		{ "data/character/yuyuko/spellEa000.cv2", 0x3890d5bf9629b630 },
		{ "data/character/yuyuko/spellEa002.cv2", 0x7d8f0745dc94276d },
		{ "data/character/yuyuko/spellEa003.cv2", 0xa9904df7c0e2975d },
		{ "data/character/yuyuko/spellEa004.cv2", 0xdf65f93af1bb79ee },
		{ "data/character/yuyuko/spellEa005.cv2", 0x7fb62550e16e8756 },
		{ "data/character/yuyuko/spellEa006.cv2", 0x713184637703e7e7 },
		{ "data/character/yuyuko/spellEa007.cv2", 0xcb23a83ebf9a4dbd },
		{ "data/character/yuyuko/spellEa008.cv2", 0x81b5e19e7894f2d7 },
		{ "data/character/yuyuko/spellEa009.cv2", 0xdc35d80af5a7445e },
		{ "data/character/yuyuko/spellEa010.cv2", 0x452ef81930c11230 },
		{ "data/character/yuyuko/spellEa011.cv2", 0xee5b71a6c080f9a2 },
		{ "data/character/yuyuko/spellGa002.cv2", 0x8387c356575f20c2 },
		{ "data/character/yuyuko/spellIa000.cv2", 0xf3d9d6b33e1c8dd6 },
		{ "data/character/yuyuko/spellIa001.cv2", 0x283e3ecb7cf288d8 },
		{ "data/character/yuyuko/spellIa002.cv2", 0x4ede63aff74629e },
		{ "data/character/yuyuko/spellIa003.cv2", 0xf0055ebfc00f4fae },
		{ "data/character/yuyuko/spellIa004.cv2", 0x9bff1c3998bf29c },
		{ "data/character/yuyuko/spellIa005.cv2", 0xc3206ce0daae1cbf },
		{ "data/character/yuyuko/spellIa006.cv2", 0x569e3e199ce0a69c },
		{ "data/character/yuyuko/spellIa007.cv2", 0x4b67362e2756e93b },
		{ "data/character/yuyuko/spellIa008.cv2", 0x6d522499049f2e21 },
		{ "data/character/yuyuko/yuyuko.pat", 0xc186737230008e48 },
		{ "data/csv/alice/spellcard.cv1", 0x2caad5a88abcb62d },
		{ "data/csv/aya/spellcard.cv1", 0x6308dd12574d9c5b },
		{ "data/csv/background/sky.cv1", 0x8a8c937af203cd50 },
		{ "data/csv/common/spellcard.cv1", 0x2e5a241f755db065 },
		{ "data/csv/iku/spellcard.cv1", 0x7a017c6ab2645311 },
		{ "data/csv/komachi/spellcard.cv1", 0x771aa0aa33541748 },
		{ "data/csv/marisa/spellcard.cv1", 0x394eeaffb82217ae },
		{ "data/csv/meirin/spellcard.cv1", 0x78176071093c0f96 },
		{ "data/csv/patchouli/spellcard.cv1", 0x5f16a34793286fc8 },
		{ "data/csv/reimu/spellcard.cv1", 0x1190f78058fa8ca6 },
		{ "data/csv/remilia/spellcard.cv1", 0x86c544c892d818fc },
		{ "data/csv/sakuya/spellcard.cv1", 0x60d188d14faa2134 },
		{ "data/csv/sanae/spellcard.cv1", 0x65c33df0799236cd },
		{ "data/csv/suika/spellcard.cv1", 0x92fc75c2aadab49f },
		{ "data/csv/suwako/spellcard.cv1", 0x3946fa8273997eef },
		{ "data/csv/tenshi/spellcard.cv1", 0xd2cbe8f52770b1b9 },
		{ "data/csv/udonge/spellcard.cv1", 0xe5aafdef2ed44008 },
		{ "data/csv/utsuho/spellcard.cv1", 0x3415624b9d4f999b },
		{ "data/csv/yukari/spellcard.cv1", 0x1e490f79f7f9e102 },
		{ "data/csv/yuyuko/spellcard.cv1", 0x435bc11dce6eac5d },
		{ "data/profile/deck2/name/sanae.cv2", 0x9da7238e1f791dd2 },
		{ "data/scenario/alice/win.cv0", 0xbf0a78e9b6cba76c },
		{ "data/scenario/aya/win.cv0", 0xfde29c7ab6f4e066 },
		{ "data/scenario/chirno/win.cv0", 0x57e43edeb0212993 },
		{ "data/scenario/iku/win.cv0", 0x62a481b1df1f1422 },
		{ "data/scenario/komachi/win.cv0", 0xf124b6153a506828 },
		{ "data/scenario/marisa/win.cv0", 0x4b13ead728efab4c },
		{ "data/scenario/meirin/win.cv0", 0xe71a3b662f1e9192 },
		{ "data/scenario/patchouli/win.cv0", 0x5a51b22cc5fa0f09 },
		{ "data/scenario/reimu/win.cv0", 0xdad0e98377926e82 },
		{ "data/scenario/remilia/win.cv0", 0x4e73c567c630734 },
		{ "data/scenario/sakuya/win.cv0", 0x82036753437682c6 },
		{ "data/scenario/sanae/win.cv0", 0x243f9340a8365d16 },
		{ "data/scenario/suika/win.cv0", 0x7269b648e3df0196 },
		{ "data/scenario/suwako/win.cv0", 0x25886e555f0e58cb },
		{ "data/scenario/tenshi/win.cv0", 0xc3cee5ad9f93603b },
		{ "data/scenario/udonge/win.cv0", 0xca85edbfbe5ff0fa },
		{ "data/scenario/utsuho/win.cv0", 0xd6a3757aeb838fe4 },
		{ "data/scenario/youmu/win.cv0", 0x7df023f713bedc7f },
		{ "data/scenario/yukari/win.cv0", 0x961b8db757a211c3 },
		{ "data/scenario/yuyuko/win.cv0", 0xa73b3dd1c577b84 },
		{ "data/scene/select/character/utsuho/utsuho002.cv2", 0xab5b7199a711b58a },
		{ "data/scene/select/character/utsuho/utsuho015.cv2", 0x99d026a31a5562c2 },
		{ "data/scene/title/2_menu_moji1.cv2", 0x70fd30f2a95646ae },
		{ "data/scene/title/2_menu_moji2.cv2", 0xad7d5b2ead37a101 },
		{ "data/se/marisa/001.cv3", 0xaaab438cdeaf3ec8 },
		{ "data/se/marisa/002.cv3", 0x2be30815d2786346 },
		{ "data/se/marisa/004.cv3", 0xe47daffa3e8f67cd },
		{ "data/se/marisa/005.cv3", 0xc985f3ca41ad56ca },
		{ "data/se/marisa/006.cv3", 0x70827aef7bf04209 },
		{ "data/se/marisa/009.cv3", 0xac5e3ffbed6e8eae },
		{ "data/se/marisa/028.cv3", 0x67f76d9697a43a81 },
		{ "data/se/marisa/031.cv3", 0x417f327875aa2f20 },
		{ "data/se/marisa/034.cv3", 0x920e9b66d8b1ebec },
		{ "data/se/marisa/041.cv3", 0x95f9e2270d0a2d22 },
		{ "data/se/sanae/004.cv3", 0xc4164997083cc929 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th123";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th123c-test.dat";

	public ArchiveTh123Tests()
	{
		// Set up code page 932 (Shift-JIS)
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		Directory.CreateDirectory(ENTRIES_PATH);
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\th123c.dat")]
	public void ReadArchiveTh123(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.Hiso, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH105.Archive>(archive);
		Assert.True(archive.Entries.Count() >= s_entryHashes.Count);

		Assert.All(archive.Entries, entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(entry.Offset <= int.MaxValue);
			Assert.NotStrictEqual(string.Empty, entry.FileName);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlySpan<byte> entryData = archive.Extract(entry);

			Assert.False(entryData.IsEmpty);
			Assert.StrictEqual(entry.Size, entryData.Length);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData));
		});
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\th123c.dat", true)]
	public async Task ReadArchiveTh123Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.Hiso, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH105.Archive>(archive);
		Assert.True(archive.Entries.Count() >= s_entryHashes.Count);

		await Assert.AllAsync(archive.Entries, async entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(entry.Offset <= int.MaxValue);
			Assert.NotStrictEqual(string.Empty, entry.FileName);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlyMemory<byte> entryData = await archive.ExtractAsync(entry);

			Assert.False(entryData.IsEmpty);
			Assert.StrictEqual(entry.Size, entryData.Length);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData.Span));

			if (writeEntriesToDisk)
			{
				string entryPath = Path.Combine(ENTRIES_PATH, entry.FileName);

				if (!File.Exists(entryPath))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);

					await using FileStream entryStream = new(entryPath, FileUtils.AsyncOpenWriteFileStreamOptions);
					await entryStream.WriteAsync(entryData);
				}
			}
		});
	}

	[Theory]
	[InlineData(ENTRIES_PATH)]
	public void WriteArchiveTh123(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.Hiso, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.Hiso, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

		Assert.Distinct(archive.Entries);
		Assert.StrictEqual(entryPaths.Length, archive.Entries.Count());

		Assert.All(archive.Entries, entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(entry.Offset <= int.MaxValue);
			Assert.NotStrictEqual(string.Empty, entry.FileName);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlySpan<byte> entryData = archive.Extract(entry);

			Assert.False(entryData.IsEmpty);
			Assert.StrictEqual(entry.Size, entryData.Length);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData));
		});
	}

	[Theory]
	[InlineData(ENTRIES_PATH)]
	public async Task WriteArchiveTh123Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.Hiso, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.Hiso, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.Distinct(archive.Entries);
		Assert.StrictEqual(entryPaths.Length, archive.Entries.Count());

		await Assert.AllAsync(archive.Entries, async entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(entry.Offset <= int.MaxValue);
			Assert.NotStrictEqual(string.Empty, entry.FileName);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlyMemory<byte> entryData = await archive.ExtractAsync(entry);

			Assert.False(entryData.IsEmpty);
			Assert.StrictEqual(entry.Size, entryData.Length);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData.Span));
		});
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\th123a.dat", "data/character/chirno/stand/惑.cv2", 0x939952cd57794e54)]
	public void ReadEntryWithJapaneseNameTh123(string path, string entryName, ulong entryDataHash)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.Hiso, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Entry? entry = archive.Entries.FirstOrDefault(entry => entry!.FileName == entryName, null);

		Assert.NotNull(entry);

		Assert.True(entry.Size > 0);
		Assert.Equal(entryName, entry.FileName);
		Assert.True(entry.Offset <= int.MaxValue);

		ReadOnlySpan<byte> entryData = archive.Extract(entry);

		Assert.False(entryData.IsEmpty);
		Assert.StrictEqual(entry.Size, entryData.Length);
		Assert.StrictEqual(entryDataHash, XxHash3.HashToUInt64(entryData));
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\th123a.dat", "data/character/chirno/stand/惑.cv2", 0x939952cd57794e54, true)]
	public async Task ReadEntryWithJapaneseNameTh123Async(string path, string entryName, ulong entryDataHash, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.Hiso, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Entry? entry = archive.Entries.FirstOrDefault(entry => entry!.FileName == entryName, null);

		Assert.NotNull(entry);

		Assert.True(entry.Size > 0);
		Assert.Equal(entryName, entry.FileName);
		Assert.True(entry.Offset <= int.MaxValue);

		ReadOnlyMemory<byte> entryData = await archive.ExtractAsync(entry);

		Assert.False(entryData.IsEmpty);
		Assert.StrictEqual(entry.Size, entryData.Length);
		Assert.StrictEqual(entryDataHash, XxHash3.HashToUInt64(entryData.Span));

		if (writeEntriesToDisk)
		{
			string entryPath = Path.Combine($"{ENTRIES_PATH}-jp", entry.FileName);

			if (!File.Exists(entryPath))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);

				await using FileStream entryStream = new(entryPath, FileUtils.AsyncOpenWriteFileStreamOptions);
				await entryStream.WriteAsync(entryData);
			}
		}
	}

	[Theory]
	[InlineData($"{ENTRIES_PATH}-jp", "data/character/chirno/stand/惑.cv2", 0x939952cd57794e54)]
	public void WriteEntryWithJapaneseNameTh123(string entriesPath, string entryName, ulong entryDataHash)
	{
		using MemoryStream outputStream = new();
		Archive.Create(Game.Hiso, outputStream, entriesPath);

		using MemoryStream inputStream = new(outputStream.GetBuffer(), writable: false);
		using Archive archive = Archive.Read(Game.Hiso, inputStream);

		Entry? entry = archive.Entries.FirstOrDefault(entry => entry!.FileName == entryName, null);

		Assert.NotNull(entry);

		Assert.True(entry.Size > 0);
		Assert.Equal(entryName, entry.FileName);
		Assert.True(entry.Offset <= int.MaxValue);

		ReadOnlySpan<byte> entryData = archive.Extract(entry);

		Assert.False(entryData.IsEmpty);
		Assert.StrictEqual(entry.Size, entryData.Length);
		Assert.StrictEqual(entryDataHash, XxHash3.HashToUInt64(entryData));
	}

	[Theory]
	[InlineData($"{ENTRIES_PATH}-jp", "data/character/chirno/stand/惑.cv2", 0x939952cd57794e54)]
	public async Task WriteEntryWithJapaneseNameTh123Async(string entriesPath, string entryName, ulong entryDataHash)
	{
		await using MemoryStream outputStream = new();
		await Archive.CreateAsync(Game.Hiso, outputStream, entriesPath);

		await using MemoryStream inputStream = new(outputStream.GetBuffer(), writable: false);
		await using Archive archive = await Archive.ReadAsync(Game.Hiso, inputStream);

		Entry? entry = archive.Entries.FirstOrDefault(entry => entry!.FileName == entryName, null);

		Assert.NotNull(entry);

		Assert.True(entry.Size > 0);
		Assert.Equal(entryName, entry.FileName);
		Assert.True(entry.Offset <= int.MaxValue);

		ReadOnlyMemory<byte> entryData = await archive.ExtractAsync(entry);

		Assert.False(entryData.IsEmpty);
		Assert.StrictEqual(entry.Size, entryData.Length);
		Assert.StrictEqual(entryDataHash, XxHash3.HashToUInt64(entryData.Span));
	}

	public void Dispose() => File.Delete(ARCHIVE_OUTPUT_PATH);
}
