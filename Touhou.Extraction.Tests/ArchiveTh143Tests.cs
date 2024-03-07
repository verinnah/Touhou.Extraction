using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh143Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0xef4775e688fa2952 },
		{ "ascii_1280.anm", 0xa718f1c8eb72d61 },
		{ "ascii_960.anm", 0x770bb65aec730a8a },
		{ "bestshot.anm", 0x15dd4e6aa5c11bab },
		{ "bullet.anm", 0x96db718ebdbdad41 },
		{ "congra.anm", 0x870d26096e20bc10 },
		{ "day01.anm", 0x2cd038ae811e76ca },
		{ "day01.std", 0xab5cedbcd8d78592 },
		{ "day02.anm", 0xaabb9702bbc3dbce },
		{ "day02.std", 0x869e579dcb585dd2 },
		{ "day03.anm", 0x676a210ced7b0408 },
		{ "day03.std", 0x82a2841024475427 },
		{ "day04.anm", 0x595754798f7ca37 },
		{ "day04.std", 0x835313c134aa28e0 },
		{ "day05.anm", 0xf4080c786914cbf4 },
		{ "day05.std", 0x1b0dc0b354823476 },
		{ "day06.anm", 0x6f06f17451996851 },
		{ "day06.std", 0xe45ce5815c74ba7 },
		{ "day07.anm", 0xfa961de193f74162 },
		{ "day07.std", 0x8d143b689d773264 },
		{ "day08.anm", 0x49464beec94f4ff7 },
		{ "day08.std", 0x42f254895cb53a40 },
		{ "day09.anm", 0xadcc43018d008bb0 },
		{ "day09.std", 0x78183d93a7f98676 },
		{ "day10.anm", 0x4096b21c7d788529 },
		{ "day10.std", 0x6312e2ca882bdc5b },
		{ "default.ecl", 0xb0cbfc3ba829009e },
		{ "ec_0101.ecl", 0x270a5f28d64c3f99 },
		{ "ec_0102.ecl", 0xcb14e762721ecf3f },
		{ "ec_0103.ecl", 0xdaaa50092d9577ab },
		{ "ec_0104.ecl", 0xdfd784bb310025f9 },
		{ "ec_0105.ecl", 0x5e11d2bacbad8adc },
		{ "ec_0106.ecl", 0x4f7730de4a51311 },
		{ "ec_0201.ecl", 0x71b0ff26a3103966 },
		{ "ec_0202.ecl", 0x99c29b53e43c2090 },
		{ "ec_0203.ecl", 0x6b39acdbc9d057f8 },
		{ "ec_0204.ecl", 0x68d1b4f7ebb47895 },
		{ "ec_0205.ecl", 0xe5cfd6ee9ba6fa59 },
		{ "ec_0206.ecl", 0x6f172412f056fa6a },
		{ "ec_0301.ecl", 0x6897fab73bdc5fe6 },
		{ "ec_0302.ecl", 0x1086217ec916ad26 },
		{ "ec_0303.ecl", 0x1905ecf5c09daffa },
		{ "ec_0304.ecl", 0xf17b8ce977ddd2dc },
		{ "ec_0305.ecl", 0xf05c907a660f25cc },
		{ "ec_0306.ecl", 0xe34c39bff99bcb65 },
		{ "ec_0307.ecl", 0xc37a148f604a3bc8 },
		{ "ec_0401.ecl", 0xb3ef2c81bcf5c634 },
		{ "ec_0402.ecl", 0x468378bf2b9a8125 },
		{ "ec_0403.ecl", 0xbae5a7d6ae5569df },
		{ "ec_0404.ecl", 0x311409ef35a3a58c },
		{ "ec_0405.ecl", 0x48c4fbea915298df },
		{ "ec_0406.ecl", 0x1f731743a2402e06 },
		{ "ec_0407.ecl", 0x22d69bd6d5eb72e3 },
		{ "ec_0501.ecl", 0x6803a68dad6c1b6b },
		{ "ec_0502.ecl", 0x447debc32fb50db5 },
		{ "ec_0503.ecl", 0x62b1e7262c0265bb },
		{ "ec_0504.ecl", 0x73516ea8b5b7c796 },
		{ "ec_0505.ecl", 0xff7f833bd284651e },
		{ "ec_0506.ecl", 0x8f7a40eda6a0a8cf },
		{ "ec_0507.ecl", 0xab77827da5dd32a3 },
		{ "ec_0508.ecl", 0x188588f5d478b23f },
		{ "ec_0601.ecl", 0x220a9ae3313adbb7 },
		{ "ec_0602.ecl", 0x3f44f93c471d361d },
		{ "ec_0603.ecl", 0xf8db26bb50a4e6f3 },
		{ "ec_0604.ecl", 0x378be361fe07bf54 },
		{ "ec_0605.ecl", 0x4563eed46f6477bd },
		{ "ec_0606.ecl", 0x3d49988395e52c6a },
		{ "ec_0607.ecl", 0x89da1db6b6ab4df6 },
		{ "ec_0608.ecl", 0xc21d853e3f7039ff },
		{ "ec_0701.ecl", 0x955ffac9837e3fb6 },
		{ "ec_0702.ecl", 0x72d064132ce3eb19 },
		{ "ec_0703.ecl", 0xebd4e78d070bfa87 },
		{ "ec_0704.ecl", 0x7258442fb938f18a },
		{ "ec_0705.ecl", 0x8b89ba9a6e1d13cf },
		{ "ec_0706.ecl", 0xfffee66bfd61b88 },
		{ "ec_0707.ecl", 0x253ec720406f282a },
		{ "ec_0708.ecl", 0xc46641178fcb4b54 },
		{ "ec_0801.ecl", 0x459f7a6a84d2b79a },
		{ "ec_0802.ecl", 0x2524a7926a382ada },
		{ "ec_0803.ecl", 0x31c0e7acfb061892 },
		{ "ec_0804.ecl", 0x4e5e8e927c3a4658 },
		{ "ec_0805.ecl", 0x3daef58e89f321bf },
		{ "ec_0806.ecl", 0xf2705d09164632a2 },
		{ "ec_0807.ecl", 0x7365f1587d676e2f },
		{ "ec_0901.ecl", 0x5a1c50540d79e98a },
		{ "ec_0902.ecl", 0x834ccabfd7766e0a },
		{ "ec_0903.ecl", 0x75b29e21e513106f },
		{ "ec_0904.ecl", 0xb70388f4c3141eae },
		{ "ec_0905.ecl", 0x9e78ce125a25e017 },
		{ "ec_0906.ecl", 0xfbbe6aee661de45 },
		{ "ec_0907.ecl", 0x57d19fb4e84f333 },
		{ "ec_0908.ecl", 0x68351d1544e0cb61 },
		{ "ec_1001.ecl", 0x466258ae00de8aa9 },
		{ "ec_1002.ecl", 0xb41423c52f3ff287 },
		{ "ec_1003.ecl", 0x9889e79018e1ccde },
		{ "ec_1004.ecl", 0x127e7b0001e39cb9 },
		{ "ec_1005.ecl", 0x78044342cdbb4951 },
		{ "ec_1006.ecl", 0x5356dde555e88fe },
		{ "ec_1007.ecl", 0x4c4847d4ecf94346 },
		{ "ec_1008.ecl", 0x1faf33261a0e84 },
		{ "ec_1009.ecl", 0xf4c4e7611e471a50 },
		{ "ec_1010.ecl", 0x8e1372c2c2c4baa },
		{ "effect.anm", 0xfb6e8a2b931b6581 },
		{ "enemy.anm", 0xc64d214f88d4bb3c },
		{ "enm01.anm", 0xff46cecc86a5dd0a },
		{ "enm01b.anm", 0x9606821e2e2a14fe },
		{ "enm02.anm", 0x7fb92df72d08e226 },
		{ "enm03.anm", 0xb52c5b89a0f6d5c1 },
		{ "enm04.anm", 0xf225cf846f2617f6 },
		{ "enm05.anm", 0x630ec64bbc676568 },
		{ "enm06.anm", 0x90e7175cb3bc0641 },
		{ "enm06b.anm", 0x873e40d606c88fff },
		{ "enm07.anm", 0x88af668ac39310c7 },
		{ "enm08.anm", 0x92b62c18c782c52e },
		{ "enm09.anm", 0xef35dd9b25b5d3d3 },
		{ "enm10.anm", 0x264aa2d7356f295a },
		{ "enm11.anm", 0x7625e35f5deff834 },
		{ "enm12.anm", 0xbc95d64eb1d2bfa4 },
		{ "enm12b.anm", 0xa2d99441d0f6e684 },
		{ "enm13.anm", 0x3d0ec1dff7c6644e },
		{ "enm14.anm", 0x5cd3a0a04c67953e },
		{ "enm14b.anm", 0xd6a602b3383579f5 },
		{ "enm15.anm", 0x174c4f8b0af70f64 },
		{ "enm16.anm", 0x9bc594f2e750520b },
		{ "enm17.anm", 0x9367695f2b143623 },
		{ "enm18.anm", 0x8e41628e35748f54 },
		{ "enm19.anm", 0xeae640859e6b82b1 },
		{ "enm20.anm", 0xf9076b87f73ccc4b },
		{ "enm21.anm", 0xfd4ed8ddbf9c0b2c },
		{ "enm22.anm", 0xb47b842cb0b11470 },
		{ "enm23.anm", 0x8f40e8cdedc599f0 },
		{ "enm23b.anm", 0x13bf242c9fcad2b4 },
		{ "enm24.anm", 0x1fdcd31cc4bbb4f3 },
		{ "enm25.anm", 0x38e6b5e300f4a333 },
		{ "enm26.anm", 0xba611ca4ea8e243 },
		{ "enm27.anm", 0x1c284fe28f34b55 },
		{ "enm28.anm", 0x711189d7c5937f97 },
		{ "enm29.anm", 0xc0afce53470d7df2 },
		{ "enm30.anm", 0x87cfca28d9b37fe },
		{ "enm31.anm", 0xe3ba6648d01291db },
		{ "enm32.anm", 0x6e762648f8d5b02f },
		{ "enm33.anm", 0xc045b690cb049370 },
		{ "front.anm", 0xc7bbb15bf16c88b9 },
		{ "help.anm", 0xf4ec8b4518d285b4 },
		{ "help_01.png", 0x997be0a3d74f832d },
		{ "help_02.png", 0x6e5dd59a09b08449 },
		{ "help_03.png", 0x2416a488b9621787 },
		{ "help_04.png", 0xd8fe4f08acad0cee },
		{ "help_05.png", 0x3e0742c506da99b7 },
		{ "help_06.png", 0xa134782c5ef39ca6 },
		{ "help_07.png", 0x239f08eecbff08f },
		{ "hint.txt", 0xd944b4c1f5bb9dc1 },
		{ "itemequip.anm", 0x53e295a148b8d67e },
		{ "msg01.msg", 0x7015d01ece7bcc7a },
		{ "msg03.msg", 0x3620d83b6866df72 },
		{ "msg05.msg", 0xd0ad1e743e209eb1 },
		{ "msg06.msg", 0xa87ffafee8472dff },
		{ "msg08.msg", 0x2a920fabac72206 },
		{ "musiccmt.txt", 0xf1189f6b579f260e },
		{ "notice.anm", 0x568500590f1978d9 },
		{ "notice_01.png", 0x69cea8ae64cf3d7b },
		{ "notice_02.png", 0x13adf718b0b8a0d4 },
		{ "notice_03.png", 0xb9dbed2047f1bc8f },
		{ "notice_04.png", 0xef15d589c2301bef },
		{ "notice_05.png", 0xa0c50b1cecf9347 },
		{ "notice_06.png", 0xc0b5b32952622e10 },
		{ "notice_07.png", 0x771b04bfe8bccec2 },
		{ "notice_08.png", 0xcdb8e9d687fcb99f },
		{ "notice_09.png", 0xdff9e1701e37febf },
		{ "notice_10.png", 0x68c83d86cd08758 },
		{ "notice_11.png", 0x24444d0ec35c0803 },
		{ "notice_12.png", 0x85c30efba8be01d6 },
		{ "notice_13.png", 0xd062f1550753cbf5 },
		{ "notice_14.png", 0xfbc756f1ef79577c },
		{ "notice_15.png", 0x2819834a802c85cf },
		{ "notice_16.png", 0xa1099119f49bbdfd },
		{ "notice_17.png", 0xe235bda5c5ee5f68 },
		{ "notice_18.png", 0x7b86553a064a374e },
		{ "notice_19.png", 0x761e31e2ff88bad0 },
		{ "notice_20.png", 0x35e97460298431ba },
		{ "notice_21.png", 0x8a9a60a38f83c74e },
		{ "notice_25.png", 0xa0a5b8c278bbc3ee },
		{ "notice_26.png", 0x78f89d59cb0be758 },
		{ "notice_27.png", 0x68170ede086be9e9 },
		{ "notice_30.png", 0xc80291d7faa3a3b5 },
		{ "notice_31.png", 0xfcfc3a4c59f30bcb },
		{ "notice_32.png", 0xf87c3f948c9c6f9f },
		{ "notice_33.png", 0xbcb283ed3e6b93b9 },
		{ "notice_34.png", 0x8be31cd004da3cf3 },
		{ "notice_35.png", 0xf2484875941f49de },
		{ "notice_36.png", 0x68df8168d254476e },
		{ "notice_37.png", 0xd2c966080e8e8624 },
		{ "notice_38.png", 0x7c2775d81e1b6407 },
		{ "notice_bk.png", 0x8d9e67dffcf17eb },
		{ "pl00.anm", 0xe0ec4cce5d069705 },
		{ "pl00a.sht", 0x4a9e81dd1c655c25 },
		{ "scene01.anm", 0x85a8dced6b997d79 },
		{ "scene01.png", 0x64c8c5a9952421eb },
		{ "scene02.anm", 0x8de81574dcbe14ce },
		{ "scene02.png", 0xa0abf7840b6fd86 },
		{ "scene03.anm", 0x856918cf34d4b11d },
		{ "scene03.png", 0xbc1f10d4187ac588 },
		{ "scene04.anm", 0xe97f84ba5b42e989 },
		{ "scene04.png", 0x1028b9198603f6e7 },
		{ "scene05.anm", 0xd2d1953345d01013 },
		{ "scene05.png", 0x1a163c7da7da1c1a },
		{ "scene06.anm", 0x4576a529c1caa469 },
		{ "scene06.png", 0xd8af62b3c6ab91ff },
		{ "scene07.anm", 0xc27332550d5e5ff9 },
		{ "scene07.png", 0xf28e3ad02079f5b8 },
		{ "scene08.anm", 0xb5249b351aa2f519 },
		{ "scene08.png", 0x71c4db3653a59ee7 },
		{ "scene09.anm", 0xf48b0f56ccda80d1 },
		{ "scene09.png", 0x6d9dbd1d6fcfcf20 },
		{ "scene10.anm", 0x694fce4a90e6fbd0 },
		{ "scene10.png", 0x4a822ccc4883fd5f },
		{ "screenswitch.anm", 0xefdcbf7eba5863c5 },
		{ "se_big.wav", 0xfb4ba3e45749198c },
		{ "se_bonus.wav", 0x333404301c4fc857 },
		{ "se_bonus2.wav", 0x368a5c8f1294dd5c },
		{ "se_bonus4.wav", 0xab65a052ae231a27 },
		{ "se_boon00.wav", 0x873bf73d48901836 },
		{ "se_boon01.wav", 0xb6e61cc0a10b2a47 },
		{ "se_bun.wav", 0x3677f23dfd985083 },
		{ "se_cancel00.wav", 0x313cef4aa5505ad1 },
		{ "se_cardget.wav", 0x4f819b02eb7afab4 },
		{ "se_cat00.wav", 0x7495554f453ba1af },
		{ "se_ch00.wav", 0x177cbb5247feaf37 },
		{ "se_ch01.wav", 0x48999fd2fcd647da },
		{ "se_ch02.wav", 0x4514aa1d8424c7e2 },
		{ "se_ch03.wav", 0x336f51977bc49eca },
		{ "se_cong.wav", 0xba30df64a051364 },
		{ "se_damage00.wav", 0x6600b8e43570fa8d },
		{ "se_damage01.wav", 0x4c72fdc517387fff },
		{ "se_don00.wav", 0x63c0b2aeacd1e2a8 },
		{ "se_enep00.wav", 0x4070801026567dbc },
		{ "se_enep01.wav", 0xa9512fb3297e92ea },
		{ "se_enep02.wav", 0xccecfaf28e083794 },
		{ "se_etbreak.wav", 0x431e75acdafd24d6 },
		{ "se_extend.wav", 0x80fc940fa2f0ed20 },
		{ "se_extend2.wav", 0x8c587999087b4057 },
		{ "se_fault.wav", 0xc39c65e2e184195a },
		{ "se_focus.wav", 0xf8014b382298e0f0 },
		{ "se_goast1.wav", 0xea1877df5d053e0f },
		{ "se_goast2.wav", 0xc2bed5109741e633 },
		{ "se_graze.wav", 0xe90a8e4fea7ce129 },
		{ "se_gun00.wav", 0xe1b3957205c72b7 },
		{ "se_heal.wav", 0xa7e6ed8a265161c5 },
		{ "se_invalid.wav", 0x429c1fa2823f6c10 },
		{ "se_item00.wav", 0x7b879e6f0f3274c7 },
		{ "se_kira00.wav", 0x14e39aa59675931b },
		{ "se_kira01.wav", 0x11f6fedced1fea00 },
		{ "se_kira02.wav", 0xa608ce966fae0898 },
		{ "se_lazer00.wav", 0x9c010a6fd5aadc25 },
		{ "se_lazer01.wav", 0x5ef637b48e701396 },
		{ "se_lazer02.wav", 0x4a056aa97b4c53d3 },
		{ "se_lgods1.wav", 0x826111b4bd43576a },
		{ "se_lgods2.wav", 0x7f5ab5a28d4bf7ec },
		{ "se_lgods3.wav", 0x71e9cd949e97a969 },
		{ "se_lgods4.wav", 0xad856a2857b1f82c },
		{ "se_lgodsget.wav", 0xa5d3140d23ae97a },
		{ "se_msl.wav", 0xa051455a80988408 },
		{ "se_msl2.wav", 0xf7ad9edb01f32546 },
		{ "se_msl3.wav", 0xd049a9c6c2ed2ac5 },
		{ "se_nep00.wav", 0xda4e266afe0b1370 },
		{ "se_nodamage.wav", 0xc186b0bb70f73fb8 },
		{ "se_noise.wav", 0x883a93ad91100247 },
		{ "se_notice.wav", 0x313ab006c69a541c },
		{ "se_ok00.wav", 0x211c94183d368af0 },
		{ "se_pause.wav", 0xf1ec61b783b86910 },
		{ "se_pin00.wav", 0xba49fda931e39a6 },
		{ "se_pin01.wav", 0x6c9fa1b12d5f1b45 },
		{ "se_pldead00.wav", 0x1d3315cbdfbab9e9 },
		{ "se_pldead01.wav", 0xb21e8f6cf05e07c },
		{ "se_plst00.wav", 0xed6645fe2e41e0c },
		{ "se_power0.wav", 0xa8311a42586a4915 },
		{ "se_power1.wav", 0xd89bb75d35046555 },
		{ "se_powerup.wav", 0xb66702f933cb7b14 },
		{ "se_poyon.wav", 0x4c7692d776e4a750 },
		{ "se_select00.wav", 0xe8a68aca740db942 },
		{ "se_shutter.wav", 0x18275555311e4ea0 },
		{ "se_slash.wav", 0x47035821cb00e36d },
		{ "se_tan00.wav", 0xa102dc9427328bb2 },
		{ "se_tan01.wav", 0xc33ecd338cbc6caf },
		{ "se_tan02.wav", 0x5f00638b5531716 },
		{ "se_tan03.wav", 0x73387ed64c1c8f3a },
		{ "se_timeout.wav", 0x7759a1efbe58daf4 },
		{ "se_timeout2.wav", 0x82699d4c0c0a2be5 },
		{ "se_trophy.wav", 0x1cf3838471ae12d2 },
		{ "se_warp_l.wav", 0x3f46b12f94b42d43 },
		{ "se_warp_n.wav", 0x6f39952f415dc982 },
		{ "se_warp_r.wav", 0x8940e22b4364b080 },
		{ "se_wolf.wav", 0x7608147ada84306 },
		{ "sig.anm", 0x528257be3665f27 },
		{ "text.anm", 0x82140916122dd3f0 },
		{ "th143_0100a.ver", 0xdcd2c819e9819df8 },
		{ "thbgm.fmt", 0x169f5b82d8e98dd5 },
		{ "title.anm", 0x5b440275c69b59b0 },
		{ "title_v.anm", 0xcafb3eb748a6d220 },
		{ "titlemsg.txt", 0x5af31a675785df17 },
		{ "trophy.anm", 0x18a1f512d44aaa51 },
		{ "trophy.txt", 0xe21f7e460447315f },
		{ "tutorial01.png", 0x550baf151ddf1012 },
		{ "tutorial02.png", 0x8ba4b618716a780 },
		{ "tutorial03.png", 0xc0e944dc3823a994 },
		{ "tutorial04.png", 0xaf12d328db8c6f68 },
		{ "tutorial05.png", 0x4b2a3ae98a3ecfe7 },
		{ "tutorial06.png", 0x7018fcc7e87e2688 },
		{ "tutorial07.png", 0x393344ab446535eb },
		{ "tutorial08.png", 0xd7fd2bf34b6e58e3 },
		{ "tutorial09.png", 0x46d5bb00e6972b57 },
		{ "tutorial10.png", 0xc4c36dbf6a90c4da },
		{ "tutorial11.png", 0x3cdeded0e870d476 },
		{ "tutorial12.png", 0x822c0a039c40631d },
		{ "tutorial13.png", 0x81df4f0ed6e75ff6 },
		{ "tutorial14.png", 0xcab34b34df4e6a0 },
		{ "tutorial15.png", 0xd24ca7bad644de29 },
		{ "tutorial16.png", 0x2aa3f750f7308045 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th143";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th143-test.dat";

	public ArchiveTh143Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th143.dat")]
	public void ReadArchiveTh143(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.ISC, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th143.dat", true)]
	public async Task ReadArchiveTh143Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.ISC, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh143(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.ISC, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.ISC, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh143Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.ISC, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.ISC, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
