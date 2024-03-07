using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh125Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0x7908667296ad7ec },
		{ "bullet.anm", 0x1c1845fcd14e8ca9 },
		{ "clear0.anm", 0x5326d24e38f6dcaf },
		{ "clear1.anm", 0x61320bd07de6febe },
		{ "default.ecl", 0x76ab2c554c2fb1e1 },
		{ "demo0.rpy", 0xe9c97e0f4656850 },
		{ "demo1.rpy", 0xaf40009a86e43237 },
		{ "demo2.rpy", 0xd1eae9541f15ed46 },
		{ "demo3.rpy", 0x35c98c7426dd2200 },
		{ "ecl10_a.ecl", 0x38e7cca7ab59aaaf },
		{ "ecl10_b.ecl", 0x15a4022fea93b8f4 },
		{ "ecl10_c.ecl", 0x3b8dd09ac45807a6 },
		{ "ecl10_d.ecl", 0xe8df0c7932189fc6 },
		{ "ecl11_a.ecl", 0xfdddab0bf344989a },
		{ "ecl11_b.ecl", 0x1ebbf3414afadf35 },
		{ "ecl11_c.ecl", 0x33180a078a08dbb3 },
		{ "ecl11_d.ecl", 0xa6f88abc5bc205a2 },
		{ "ecl12_a.ecl", 0x59ad514445bc7a2c },
		{ "ecl12_b.ecl", 0x3c4b308c2467116f },
		{ "ecl12_c.ecl", 0x8c540064545a6ef1 },
		{ "ecl12_d.ecl", 0xbedbd5d0d26c03ee },
		{ "ecl13_a.ecl", 0xd32800ecfdd55974 },
		{ "ecl13_b.ecl", 0x1faa9d01c36a78aa },
		{ "ecl13_c.ecl", 0xe8ad0fa8aa9e071d },
		{ "ecl13_d.ecl", 0x235702770678286d },
		{ "ecl14_a.ecl", 0x40a7a97269bad44a },
		{ "ecl14_b.ecl", 0xecbea002184fdfd3 },
		{ "ecl14_c.ecl", 0xb47059273920881e },
		{ "ecl14_d.ecl", 0xceb2657afd3141f },
		{ "ecl15_a.ecl", 0xa6c0ffbd01916c18 },
		{ "ecl15_b.ecl", 0x49f92c321695188c },
		{ "ecl15_c.ecl", 0x6c851440a1c11952 },
		{ "ecl16_a.ecl", 0xd36474f2dce4c424 },
		{ "ecl16_b.ecl", 0x47c8c51e3e35920f },
		{ "ecl16_c.ecl", 0xf6a7358b6ca4f125 },
		{ "ecl16_d.ecl", 0x2fb7ca272a92a72f },
		{ "ecl17_a.ecl", 0x7800b24c7c9aadb1 },
		{ "ecl17_b.ecl", 0x3e94d18ac60470db },
		{ "ecl17_c.ecl", 0xe0b433c557e26746 },
		{ "ecl17_d.ecl", 0xfb629ea5dfc645d7 },
		{ "ecl18_a.ecl", 0x56b5a5a9f94c3a8f },
		{ "ecl18_b.ecl", 0xa8e995a2c4216153 },
		{ "ecl18_c.ecl", 0x346dc78a3a51b3e6 },
		{ "ecl18_d.ecl", 0x2cc0eb5eb008a013 },
		{ "ecl19_a.ecl", 0xa84c9d68069a3ba },
		{ "ecl19_b.ecl", 0xc1e6676bc95c6b24 },
		{ "ecl19_c.ecl", 0xa891cfbe8e70b580 },
		{ "ecl19_d.ecl", 0x728a4012b992512c },
		{ "ecl1_a.ecl", 0x5de7436ce5ae3328 },
		{ "ecl1_b.ecl", 0xaf50216a1e6dea31 },
		{ "ecl1_c.ecl", 0x54d3a48d0b185041 },
		{ "ecl1_d.ecl", 0xf605402a5c660c7f },
		{ "ecl20_a.ecl", 0x6f83ecbf3c320018 },
		{ "ecl20_b.ecl", 0xe7d9bd9debb07d7e },
		{ "ecl20_c.ecl", 0x60eb842b40d643d0 },
		{ "ecl20_d.ecl", 0xe905d965c3b47784 },
		{ "ecl21_a.ecl", 0xdb9fce8f00779a19 },
		{ "ecl21_b.ecl", 0xb694f6ae63a676c1 },
		{ "ecl21_c.ecl", 0x3fb123b15cbdef57 },
		{ "ecl21_d.ecl", 0xbc9ccfff12e94fe6 },
		{ "ecl22_a.ecl", 0x9f217f4cbe6bac8 },
		{ "ecl22_b.ecl", 0xa0e80f0416566d9a },
		{ "ecl22_c.ecl", 0x2f29ebb493addcac },
		{ "ecl22_d.ecl", 0x1956563929b52fd },
		{ "ecl23_a.ecl", 0xe171fd229ae345c7 },
		{ "ecl23_b.ecl", 0x7873e493fb70b470 },
		{ "ecl23_c.ecl", 0xd23a5c1a27e25a88 },
		{ "ecl23_d.ecl", 0x5fcdc8d397aa3e48 },
		{ "ecl24_a.ecl", 0xfb525c7058433f1f },
		{ "ecl24_b.ecl", 0x10dab2b446f96388 },
		{ "ecl24_c.ecl", 0x75c61fa6f2bca0ef },
		{ "ecl24_d.ecl", 0x4cd38c9d7106b51c },
		{ "ecl25_a.ecl", 0x3c306418cbe0b1cd },
		{ "ecl25_b.ecl", 0xbb37c102da1fdd25 },
		{ "ecl25_c.ecl", 0xc40eafc64768432f },
		{ "ecl25_d.ecl", 0x934da5324976f72f },
		{ "ecl26_a.ecl", 0xf8678510047e9a17 },
		{ "ecl26_b.ecl", 0x801fd7921f91b71d },
		{ "ecl26_c.ecl", 0x2adc751e1ff442a6 },
		{ "ecl27_a.ecl", 0xeeab0fc917881dd9 },
		{ "ecl27_b.ecl", 0x9491cd14ba4baf19 },
		{ "ecl27_c.ecl", 0x9343a9b9639b59d5 },
		{ "ecl28_a.ecl", 0x28aea46b152df42c },
		{ "ecl28_b.ecl", 0x72d50cd0a0c1b673 },
		{ "ecl28_c.ecl", 0xb783728f3da76c8b },
		{ "ecl29_a.ecl", 0xb55f8d35c386becc },
		{ "ecl29_b.ecl", 0x634e6ac09ba625c2 },
		{ "ecl29_c.ecl", 0xfc36334f88f9857b },
		{ "ecl29_d.ecl", 0xa6c61871580a70e2 },
		{ "ecl2_a.ecl", 0x67a932b33a910618 },
		{ "ecl2_b.ecl", 0x26ac52b26de52921 },
		{ "ecl30_a.ecl", 0xec2fce4d101afb70 },
		{ "ecl30_b.ecl", 0x6423ec55d8eb03d1 },
		{ "ecl30_c.ecl", 0x6a3b44031e05e7e },
		{ "ecl30_d.ecl", 0xf3f19c30de71cf10 },
		{ "ecl30_e.ecl", 0xdd73f05cf379ae22 },
		{ "ecl3_a.ecl", 0x36365020ee994470 },
		{ "ecl3_b.ecl", 0x48b41c7c66b06fbc },
		{ "ecl3_c.ecl", 0x115578807fc07235 },
		{ "ecl4_a.ecl", 0xcaf8b54323bfadd1 },
		{ "ecl4_b.ecl", 0x7db1b879e00dc824 },
		{ "ecl4_c.ecl", 0xdf771f118de2b2ac },
		{ "ecl5_a.ecl", 0x63764396219137bf },
		{ "ecl5_b.ecl", 0x26d24ecb9159b691 },
		{ "ecl5_c.ecl", 0xd15d1a8b2ef9ca17 },
		{ "ecl6_a.ecl", 0x7273a46903383dfb },
		{ "ecl6_b.ecl", 0xc23ba0b171cf66c8 },
		{ "ecl7_a.ecl", 0xbee11cf55e9cad4c },
		{ "ecl7_b.ecl", 0xcdd8d3d44da08616 },
		{ "ecl7_c.ecl", 0x1cbe16ce9c6a1a64 },
		{ "ecl8_a.ecl", 0x802f6fd2268c6972 },
		{ "ecl8_b.ecl", 0x829ad17ac08ab103 },
		{ "ecl8_c.ecl", 0x61e5dc7068bdf128 },
		{ "ecl8_d.ecl", 0xccd71cec31b18632 },
		{ "ecl9_a.ecl", 0xca90bbde22357327 },
		{ "ecl9_b.ecl", 0xede46b502d163e8f },
		{ "ecl9_c.ecl", 0x7ca0604cf31e121a },
		{ "enemy.anm", 0x446310e5540c69d3 },
		{ "enm1.anm", 0xee70a1676cba4831 },
		{ "enm10.anm", 0x6c4a26515a440934 },
		{ "enm11.anm", 0x673c217e530a4b21 },
		{ "enm12.anm", 0xab46476938645b86 },
		{ "enm13.anm", 0x496eac7ce9b74611 },
		{ "enm14.anm", 0xea116f74877c0a3a },
		{ "enm15.anm", 0x6e32b7520c31b638 },
		{ "enm16.anm", 0xc59dcd32776a7659 },
		{ "enm17.anm", 0xde3b0f526ffc8c9c },
		{ "enm18.anm", 0x495215c285536e81 },
		{ "enm19.anm", 0x22b78c3d41d781e9 },
		{ "enm20.anm", 0x4d22ffa573aba623 },
		{ "enm21.anm", 0x995b2fb344b9a32b },
		{ "enm22.anm", 0x70d67d429658b9dc },
		{ "enm23.anm", 0x459ef228a40409d6 },
		{ "enm24.anm", 0x58ab596e74c6bb46 },
		{ "enm25.anm", 0x810f78d2c8c42b9 },
		{ "enm26.anm", 0xee21b8de752f360e },
		{ "enm27.anm", 0x6e855edd9f9dc60e },
		{ "enm28.anm", 0xa9af8197561814dc },
		{ "enm29.anm", 0xec26492b9bf6d52a },
		{ "enm3.anm", 0xda4feb763392388f },
		{ "enm30.anm", 0xaaa728f61939a647 },
		{ "enm4.anm", 0xa11a3e5b7dd0c1fc },
		{ "enm5.anm", 0xbee186b17e2db471 },
		{ "enm7.anm", 0x3761386649b2594a },
		{ "enm8.anm", 0x998fec1306d5ab48 },
		{ "enm9.anm", 0xc0aabac68cca3dee },
		{ "face0.anm", 0x8992f26d75a1d62b },
		{ "face1.anm", 0xd029926ee0641570 },
		{ "front.anm", 0x418721cafcbee848 },
		{ "help.anm", 0xf42bc75d6c07e9e0 },
		{ "help.txt", 0xfaa6351d4dd5af51 },
		{ "help_00.png", 0xab77dc9271e5d457 },
		{ "help_01.png", 0x2d6831c69080029b },
		{ "help_02.png", 0x69a0ea7f8485a15 },
		{ "help_03.png", 0xac9cc480e6cda0fb },
		{ "help_04.png", 0xfe3ea9ef7cc97777 },
		{ "help_05.png", 0x1940254710518773 },
		{ "help_06.png", 0x3c27c50b549ce218 },
		{ "help_07.png", 0x6077f238a8f009bc },
		{ "help_08.png", 0x6077f238a8f009bc },
		{ "help_09.png", 0x70a03a3f368f3cdb },
		{ "mission.msg", 0x49b3fa29f932c340 },
		{ "mission001.png", 0x3025a026d31b346 },
		{ "mission002.png", 0x7a8edc66bdee3bd0 },
		{ "mission003.png", 0x7b6f20cd6b62e0bc },
		{ "mission004.png", 0xeaa2fbbd092cadbb },
		{ "mission005.png", 0x9e7e814855ec3607 },
		{ "mission006.png", 0x8d1f8bc034f115 },
		{ "mission007.png", 0xc871ad1eb234ed32 },
		{ "mission008.png", 0x8ac7662f8b5eb264 },
		{ "mission009.png", 0xad6a2e2bc1477cd6 },
		{ "mission010.png", 0x72f57b17c749b9bc },
		{ "mission011.png", 0x90f31696804c59ff },
		{ "mission012.png", 0xe6659724d2553317 },
		{ "mission013.png", 0x7b8e750946b90bd2 },
		{ "mission014.png", 0x5e3187946f3e9017 },
		{ "mission015.png", 0x6e7962ceb413ac92 },
		{ "mission016.png", 0x7afbc202b64ab775 },
		{ "mission017.png", 0x47323987fe6be36e },
		{ "mission018.png", 0xab6ef7c13720552e },
		{ "mission019.png", 0xc391aa10b4845078 },
		{ "mission020.png", 0xf22b638d5e348c0 },
		{ "mission021.png", 0x44941e83aa0367a8 },
		{ "mission022.png", 0x375c2f93552271f4 },
		{ "mission023.png", 0x33f5357c52816832 },
		{ "mission024.png", 0x941d691c0cd703b8 },
		{ "mission025.png", 0x7fcac8d495cece6b },
		{ "mission026.png", 0xb6450e1a7242ceb },
		{ "mission027.png", 0x666cc50fc3502cfc },
		{ "mission028.png", 0xe7b47964cb9eb1b6 },
		{ "mission029.png", 0x4ca8738b0e831444 },
		{ "mission030.png", 0x1bb7b38233aa8b0b },
		{ "mission031.png", 0x7457fd0cd4990b3f },
		{ "mission101.png", 0xbceb8a5d9c6b5f58 },
		{ "mission102.png", 0xfbdb3a72726dbc62 },
		{ "mission103.png", 0xac714887310acfed },
		{ "mission104.png", 0xbb8ee3ae6bfd5a2c },
		{ "mission105.png", 0x6cecdb9e858ace13 },
		{ "mission106.png", 0xd52e6e532b68bc4b },
		{ "mission107.png", 0xdad19306cb44640 },
		{ "mission108.png", 0xe548e32f1a634e19 },
		{ "mission109.png", 0x77b9e43d9cee947f },
		{ "mission110.png", 0xd6e8cd3edcc12da7 },
		{ "mission_01.png", 0x5876f45663e90659 },
		{ "mission_02.png", 0xa6e9cf83880c27a2 },
		{ "mission_03.png", 0x50ecb20c579d9b00 },
		{ "mission_04.png", 0x955652d0d87e616c },
		{ "mission_05.png", 0x92d1de187e4c95fa },
		{ "mission_06.png", 0x107a406b8264fdf7 },
		{ "mission_07.png", 0x4e10bf6d5c207600 },
		{ "mission_08.png", 0xea99ec11e0f95e1d },
		{ "mission_09.png", 0x7709335a71c6e2fb },
		{ "mission_10.png", 0x2a11de8a48680df1 },
		{ "mission_11.png", 0xfe551bc5a55fbb1c },
		{ "mission_12.png", 0x569a309b438e6b2f },
		{ "mission_13.png", 0xf1dce8875c650671 },
		{ "mission_14.png", 0xefc2acb674eb0fc3 },
		{ "musiccmt.txt", 0xc65a9415e7ab5b5e },
		{ "photo.anm", 0x76e4eb418a2419cd },
		{ "pl00_00.png", 0x8cad524d2455dcbc },
		{ "pl00_01.png", 0x90ba8796eea48b6 },
		{ "pl00_02.png", 0x41a5dcb30b2b2dcf },
		{ "pl00_05.png", 0xb04c2f94ecccff65 },
		{ "pl00_06.png", 0x9957d7429f361136 },
		{ "pl00_07.png", 0xcf7d7f4668e6da21 },
		{ "pl01_00.png", 0x39a0b22f6a33f058 },
		{ "pl01_01.png", 0x2c10575b4055bd02 },
		{ "pl01_02.png", 0xfbf44cbf33d3e9ca },
		{ "pl01_05.png", 0x6ffcd04a16a01131 },
		{ "pl01_06.png", 0x50b1412161f9ec3 },
		{ "pl01_07.png", 0x65aaea39345a2b41 },
		{ "player.anm", 0x547ef68af05eae5 },
		{ "player.sht", 0xd3ff5fdbe9957e5 },
		{ "player2.anm", 0x7b5e4181197f08c0 },
		{ "se_bonus.wav", 0x333404301c4fc857 },
		{ "se_bonus2.wav", 0x368a5c8f1294dd5c },
		{ "se_boon00.wav", 0x873bf73d48901836 },
		{ "se_boon01.wav", 0xb6e61cc0a10b2a47 },
		{ "se_cancel00.wav", 0x313cef4aa5505ad1 },
		{ "se_cardget.wav", 0x4f819b02eb7afab4 },
		{ "se_cat00.wav", 0x7495554f453ba1af },
		{ "se_cat01.wav", 0xdafdbe921b8a187a },
		{ "se_ch00.wav", 0x177cbb5247feaf37 },
		{ "se_ch01.wav", 0x48999fd2fcd647da },
		{ "se_ch02.wav", 0x4514aa1d8424c7e2 },
		{ "se_changeitem.wav", 0x6aec872d94ad63df },
		{ "se_damage00.wav", 0x6600b8e43570fa8d },
		{ "se_damage01.wav", 0x4c72fdc517387fff },
		{ "se_don00.wav", 0x63c0b2aeacd1e2a8 },
		{ "se_enep00.wav", 0x699b01dc8d1eb6c6 },
		{ "se_enep01.wav", 0xa9512fb3297e92ea },
		{ "se_enep02.wav", 0xccecfaf28e083794 },
		{ "se_extend.wav", 0x80fc940fa2f0ed20 },
		{ "se_focus.wav", 0xf8014b382298e0f0 },
		{ "se_focusfix.wav", 0x52d7d93ec56cc756 },
		{ "se_focusfix2.wav", 0xf384b5ef0ce1e66 },
		{ "se_focusin.wav", 0xb12cc628c631ab01 },
		{ "se_focusrot.wav", 0x2fee41333e4f931a },
		{ "se_graze.wav", 0xe90a8e4fea7ce129 },
		{ "se_gun00.wav", 0xe1b3957205c72b7 },
		{ "se_invalid.wav", 0x429c1fa2823f6c10 },
		{ "se_item00.wav", 0x7b879e6f0f3274c7 },
		{ "se_item01.wav", 0x483a913fdaf532aa },
		{ "se_kira00.wav", 0x14e39aa59675931b },
		{ "se_kira01.wav", 0x11f6fedced1fea00 },
		{ "se_kira02.wav", 0xa608ce966fae0898 },
		{ "se_lazer00.wav", 0x9c010a6fd5aadc25 },
		{ "se_lazer01.wav", 0x5ef637b48e701396 },
		{ "se_lazer02.wav", 0x4a056aa97b4c53d3 },
		{ "se_nep00.wav", 0xda4e266afe0b1370 },
		{ "se_nice.wav", 0xa6ab57f4d6404778 },
		{ "se_nodamage.wav", 0xc186b0bb70f73fb8 },
		{ "se_ok00.wav", 0x211c94183d368af0 },
		{ "se_option.wav", 0xf77e827eef8f3242 },
		{ "se_pause.wav", 0xf1ec61b783b86910 },
		{ "se_piyo.wav", 0x78729a640997df96 },
		{ "se_pldead00.wav", 0x1d3315cbdfbab9e9 },
		{ "se_plst00.wav", 0xed6645fe2e41e0c },
		{ "se_power0.wav", 0xa8311a42586a4915 },
		{ "se_power1.wav", 0xd89bb75d35046555 },
		{ "se_powerup.wav", 0x18d8eb75d98e6104 },
		{ "se_select00.wav", 0xe8a68aca740db942 },
		{ "se_shutter.wav", 0x18275555311e4ea0 },
		{ "se_slash.wav", 0x47035821cb00e36d },
		{ "se_tan00.wav", 0xa102dc9427328bb2 },
		{ "se_tan01.wav", 0xc33ecd338cbc6caf },
		{ "se_tan02.wav", 0x5f00638b5531716 },
		{ "se_timeout.wav", 0x7759a1efbe58daf4 },
		{ "se_timeout2.wav", 0x82699d4c0c0a2be5 },
		{ "se_ufo.wav", 0xae45302d06837a38 },
		{ "se_ufoalert.wav", 0x61a1c8423f756073 },
		{ "sig.anm", 0x2e40b29810242eee },
		{ "sp1.msg", 0x124d491ec9a9c388 },
		{ "text.anm", 0x41b1bc55bc213705 },
		{ "th125_0100a.ver", 0x2034ec1e7e2759a },
		{ "thbgm.fmt", 0xe8370ebe99a8035b },
		{ "title.anm", 0x21d8e29b9526513f },
		{ "title_v.anm", 0xdbab3f25eeaf71d4 },
		{ "world01.anm", 0x20dfef1f4aef981a },
		{ "world01.std", 0xc89825a07260a7f2 },
		{ "world02.anm", 0x38ee88840189b4c },
		{ "world02.std", 0x38440619f0740614 },
		{ "world03.anm", 0x58e756daf99759f5 },
		{ "world03.std", 0x10f73c911c87e8dc },
		{ "world04.anm", 0x68a67d4db6151218 },
		{ "world04.std", 0x9c0d7f59e6a3bb31 },
		{ "world05.anm", 0x7990f9b942fc6557 },
		{ "world05.std", 0x11460fcd8bea9ae8 },
		{ "world06.anm", 0xc3269de771cf6170 },
		{ "world06.std", 0xf12321140e745324 },
		{ "world07.anm", 0xcd04d2bb57769043 },
		{ "world07.std", 0x4b2e18ef9f627bc7 },
		{ "world08.anm", 0x152122bbb07e87dc },
		{ "world08.std", 0x10b9daf086d88bf },
		{ "world09.anm", 0xe7790caa8d24175b },
		{ "world09.std", 0x1437d05c41862570 },
		{ "world10.anm", 0x703c7019649e10ec },
		{ "world10.std", 0xb9055a327bb7f95e },
		{ "world11.anm", 0x591b3d5f36ccb709 },
		{ "world11.std", 0xff969e82351f4bc6 },
		{ "world12.anm", 0x54030bfa24df39d2 },
		{ "world12.std", 0x678b3f3eb55cc36a },
		{ "world13.anm", 0x5c58425c64c3ed3f },
		{ "world13.std", 0x504ea607ec254df7 },
		{ "world14.anm", 0xd1fd9f7db1cf1377 },
		{ "world14.std", 0xff65658cfa6d77e9 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th125";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th125-test.dat";

	public ArchiveTh125Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th125.dat")]
	public void ReadArchiveTh125(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.DS, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH95.THA1>(archive);
		Assert.True(archive.Entries.Count() >= s_entryHashes.Count);

		Assert.All(archive.Entries, entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(((ZunEntry)entry).Offset > 0);
			Assert.True(((ZunEntry)entry).CompressedSize > 0);
			Assert.NotStrictEqual(string.Empty, entry.FileName);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlySpan<byte> entryData = archive.Extract(entry);

			Assert.False(entryData.IsEmpty);
			Assert.StrictEqual(entry.Size, entryData.Length);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData));
		});
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\th125.dat", true)]
	public async Task ReadArchiveTh125Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.DS, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH95.THA1>(archive);
		Assert.True(archive.Entries.Count() >= s_entryHashes.Count);

		await Assert.AllAsync(archive.Entries, async entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(((ZunEntry)entry).Offset > 0);
			Assert.True(((ZunEntry)entry).CompressedSize > 0);
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
					await using FileStream entryStream = new(entryPath, FileUtils.AsyncOpenWriteFileStreamOptions);
					await entryStream.WriteAsync(entryData);
				}
			}
		});
	}

	[Theory]
	[InlineData(ENTRIES_PATH)]
	public void WriteArchiveTh125(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.DS, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.DS, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

		Assert.Distinct(archive.Entries);
		Assert.StrictEqual(entryPaths.Length, archive.Entries.Count());

		Assert.All(archive.Entries, entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(((ZunEntry)entry).Offset > 0);
			Assert.True(((ZunEntry)entry).CompressedSize > 0);
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
	public async Task WriteArchiveTh125Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.DS, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.DS, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.Distinct(archive.Entries);
		Assert.StrictEqual(entryPaths.Length, archive.Entries.Count());

		await Assert.AllAsync(archive.Entries, async entry =>
		{
			Assert.True(entry.Size > 0);
			Assert.True(((ZunEntry)entry).Offset > 0);
			Assert.True(((ZunEntry)entry).CompressedSize > 0);
			Assert.NotStrictEqual(string.Empty, entry.FileName);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlyMemory<byte> entryData = await archive.ExtractAsync(entry);

			Assert.False(entryData.IsEmpty);
			Assert.StrictEqual(entry.Size, entryData.Length);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData.Span));
		});
	}

	public void Dispose() => File.Delete(ARCHIVE_OUTPUT_PATH);
}
