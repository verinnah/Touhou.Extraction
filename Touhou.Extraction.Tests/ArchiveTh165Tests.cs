using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh165Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0x1b13625fdeb23f95 },
		{ "ascii_1280.anm", 0x4e63b219af94246d },
		{ "ascii_960.anm", 0x12a9f761ecd94931 },
		{ "bullet.anm", 0x5727ddbbca5c3fb3 },
		{ "congra.anm", 0x3e37dec3bd1bc624 },
		{ "day01.anm", 0xb47af554832c8fdd },
		{ "day01.std", 0x58d5f2673ce67a23 },
		{ "day02.anm", 0xe0d9dc15e51b86df },
		{ "day02.std", 0xfd2fceb64ed27395 },
		{ "day03.anm", 0x62a645b72494053d },
		{ "day03.std", 0xd3c270c7897babd5 },
		{ "day04.anm", 0x9a854a4b9de7515b },
		{ "day04.std", 0x68cd29144081b9ed },
		{ "day05.anm", 0x9649308fdee7492e },
		{ "day05.std", 0xe00eaa4a3b609774 },
		{ "day06.anm", 0x86eef3d8ec9f0cf6 },
		{ "day06.std", 0xda7b409c5f221d84 },
		{ "day08.anm", 0xe35dba5753c624 },
		{ "day08.std", 0x85e874880404b2a6 },
		{ "day09.anm", 0x6e032356ac3fa68 },
		{ "day09.std", 0xc22ba80b28caed5f },
		{ "day10.anm", 0xe72730c670fb1dc7 },
		{ "day10.std", 0x6cf909219dca2fb7 },
		{ "day11.anm", 0x306ed132b775f940 },
		{ "day11.std", 0x7d2bbe549e5b466b },
		{ "day12.anm", 0xb70216ebf9ba39ff },
		{ "day12.std", 0xa2fe21fc7bfd46be },
		{ "day13.anm", 0xfec56ceeca1567a6 },
		{ "day13.std", 0x553738d246f44e11 },
		{ "day14.anm", 0x39839b38c02a4f1a },
		{ "day14.std", 0x43c248cc33095672 },
		{ "day15.anm", 0x7724385fcc55d272 },
		{ "day15.std", 0xd83c13ae1d603f7a },
		{ "day16.anm", 0xe0e74b75f24e2f81 },
		{ "day16.std", 0x6e39caa29d34896a },
		{ "day17.anm", 0xdc9962aab776006d },
		{ "day17.std", 0xc698b39c5605f1fb },
		{ "day18.anm", 0xbf8bfe87dc10409f },
		{ "day18.std", 0xdc411e5352de5579 },
		{ "day19.anm", 0x39bf24bf04b35055 },
		{ "day19.std", 0x1526c2e3307b1bbc },
		{ "default.ecl", 0x12a5dc8f743dbd9a },
		{ "ec_0101.ecl", 0x5846326e6c8c3275 },
		{ "ec_0102.ecl", 0x641f7cea671ededc },
		{ "ec_0201.ecl", 0x3af65bd571cd9015 },
		{ "ec_0202.ecl", 0xfd242074ff67e8c7 },
		{ "ec_0203.ecl", 0x7e5827eb9d08e0f7 },
		{ "ec_0204.ecl", 0x9c1ed476b9b833f3 },
		{ "ec_0301.ecl", 0x5d85b11ac6703789 },
		{ "ec_0302.ecl", 0x4780526c4c8abd0f },
		{ "ec_0303.ecl", 0xa06a16a269bdf50a },
		{ "ec_0401.ecl", 0x926e5bf491030ddd },
		{ "ec_0402.ecl", 0xf7c5fdbcea848505 },
		{ "ec_0403.ecl", 0x16dd8759e69b1320 },
		{ "ec_0404.ecl", 0x893f59a2fea246d5 },
		{ "ec_0501.ecl", 0x2a1d23abf76e8c9f },
		{ "ec_0502.ecl", 0x8142b28168f5a170 },
		{ "ec_0503.ecl", 0x6e4d5133e85ff23b },
		{ "ec_0601.ecl", 0x55bb786d4203d11e },
		{ "ec_0602.ecl", 0xcc6e5f7c73210ca6 },
		{ "ec_0603.ecl", 0xee7ab41d8596e155 },
		{ "ec_0701.ecl", 0x93b4173ea19e21b5 },
		{ "ec_0801.ecl", 0xf509d6dc21c9c6d1 },
		{ "ec_0802.ecl", 0xc6ae9c03e38439c3 },
		{ "ec_0803.ecl", 0xbcb1c19d961d4b03 },
		{ "ec_0804.ecl", 0xc99a3e841b5d2ebc },
		{ "ec_0805.ecl", 0x7ec8d63dc222365c },
		{ "ec_0806.ecl", 0x1c9516736669f536 },
		{ "ec_0807.ecl", 0xcc64e59bc857944b },
		{ "ec_0901.ecl", 0x4cc54b80b488308c },
		{ "ec_0902.ecl", 0xd51363b6ac7eae55 },
		{ "ec_0903.ecl", 0x5a8cbd42d94f30b6 },
		{ "ec_0904.ecl", 0xb93f3ab6d2cadd33 },
		{ "ec_1001.ecl", 0x4b4cf586d5025ada },
		{ "ec_1002.ecl", 0x6c39a0e4f8815f7a },
		{ "ec_1003.ecl", 0x60b06e5018999990 },
		{ "ec_1004.ecl", 0x3f4085bf6b7b4c7a },
		{ "ec_1101.ecl", 0xa1e94428424ffa2b },
		{ "ec_1102.ecl", 0x86036d480abb151b },
		{ "ec_1103.ecl", 0x5e204ff3f2408c3e },
		{ "ec_1104.ecl", 0x1aeaa965bde37bc1 },
		{ "ec_1105.ecl", 0xcc9d75eeaefcb8bf },
		{ "ec_1106.ecl", 0x69124e95295897db },
		{ "ec_1201.ecl", 0xf9eeccdb439749c3 },
		{ "ec_1202.ecl", 0x3f4389e21a5c7293 },
		{ "ec_1203.ecl", 0xd4600520ce6b5010 },
		{ "ec_1204.ecl", 0x6a497eb1d84f1dad },
		{ "ec_1205.ecl", 0x2a9e5162cc7b5523 },
		{ "ec_1301.ecl", 0x8b02045d5b49335d },
		{ "ec_1302.ecl", 0xcd6f1a304aa9a67b },
		{ "ec_1303.ecl", 0x84c3a6e6ff3a0852 },
		{ "ec_1304.ecl", 0x66156abee481a5d },
		{ "ec_1305.ecl", 0xa2fa74740371e153 },
		{ "ec_1401.ecl", 0xa18c924f15a9076f },
		{ "ec_1402.ecl", 0xe670a0c916ebeabc },
		{ "ec_1403.ecl", 0xa693bba0b68b79e5 },
		{ "ec_1404.ecl", 0xb33c98ea013cff7a },
		{ "ec_1405.ecl", 0xc1447f3e19c8a9f8 },
		{ "ec_1406.ecl", 0xe35c25235ee225a9 },
		{ "ec_1501.ecl", 0xb49bb1281375c30e },
		{ "ec_1502.ecl", 0x847f12be6dbb504c },
		{ "ec_1503.ecl", 0x64f42a3c24a8d183 },
		{ "ec_1504.ecl", 0x28f397d533e50cc3 },
		{ "ec_1505.ecl", 0xacb78fc7e3fe8aca },
		{ "ec_1506.ecl", 0xfe3de1bf7157003e },
		{ "ec_15a.ecl", 0xc96a0a3d706cb986 },
		{ "ec_15b.ecl", 0x40d98f9f97e2af4e },
		{ "ec_15c.ecl", 0x9b4e8c309790086b },
		{ "ec_15d.ecl", 0x31fe6217d08f0125 },
		{ "ec_1601.ecl", 0xa33e88c486367070 },
		{ "ec_1602.ecl", 0x7e1b927d3ff0af60 },
		{ "ec_1603.ecl", 0x1708ffa49a4668ef },
		{ "ec_1604.ecl", 0x150b24cad5dab16d },
		{ "ec_1605.ecl", 0x98e9d041fbd132a1 },
		{ "ec_1606.ecl", 0xfed72ee0df21133d },
		{ "ec_16a.ecl", 0x6acee34864432c3e },
		{ "ec_16b.ecl", 0x6db3defb6ba60b23 },
		{ "ec_16c.ecl", 0xa6a0fa05a95f3ac2 },
		{ "ec_16d.ecl", 0xb26708733c47e04 },
		{ "ec_1701.ecl", 0x41feed5647503ba3 },
		{ "ec_1702.ecl", 0xda7e5abfed04d7ce },
		{ "ec_1703.ecl", 0x10ca811927f882ea },
		{ "ec_1704.ecl", 0xc6da5aef15796429 },
		{ "ec_1705.ecl", 0xbe1b0996e321cf82 },
		{ "ec_1706.ecl", 0xd22cbe811112d601 },
		{ "ec_17a.ecl", 0xab1863d27c65a21d },
		{ "ec_17b.ecl", 0x4217ebd27f82c21a },
		{ "ec_17c.ecl", 0xe5371212e82453ed },
		{ "ec_17d.ecl", 0x39503461773744d6 },
		{ "ec_1801.ecl", 0x9ab51771f472414d },
		{ "ec_1802.ecl", 0x7406c02d2831ac11 },
		{ "ec_1803.ecl", 0xebcb5559807f9b50 },
		{ "ec_1804.ecl", 0x2eaa81bd6d8bb83d },
		{ "ec_1805.ecl", 0x136d5367fe0df868 },
		{ "ec_1806.ecl", 0x8cb0abb2174b2da0 },
		{ "ec_18a.ecl", 0x5971af4e9d75f770 },
		{ "ec_18b.ecl", 0x61156ae55a13145f },
		{ "ec_18c.ecl", 0x5ff1e4290692b6b2 },
		{ "ec_18d.ecl", 0xe3c419f898ed44 },
		{ "ec_1901.ecl", 0xf5dd47b8e2bc24bd },
		{ "ec_1902.ecl", 0x63d06b8fa66f4a44 },
		{ "ec_1903.ecl", 0xf87639a6a8ffcc81 },
		{ "ec_1904.ecl", 0x82ddaf71626043a6 },
		{ "ec_1905.ecl", 0x9013cddc0c92b0e },
		{ "ec_1906.ecl", 0xb1d726d9b5dff757 },
		{ "ec_19a.ecl", 0x3d982be302ccafbb },
		{ "ec_19b.ecl", 0x6fcefe9d5b6433ee },
		{ "ec_19c.ecl", 0x3f837710644dac5a },
		{ "ec_19d.ecl", 0xf44fe7d6c3286715 },
		{ "ec_2001.ecl", 0x74f18ef49d4aa10d },
		{ "ec_2002.ecl", 0x8050a52b1972db4c },
		{ "ec_2003.ecl", 0x886b05db1fb7d73c },
		{ "ec_2004.ecl", 0xa446b49d63872e7f },
		{ "ec_2005.ecl", 0x9b9b53bd1319a669 },
		{ "ec_2006.ecl", 0x77df6cab8755cb89 },
		{ "ec_20a.ecl", 0x5994e597fd737400 },
		{ "ec_20b.ecl", 0x1475b00dbf734f72 },
		{ "ec_20c.ecl", 0xeb0279265d7862f },
		{ "ec_20d.ecl", 0x4f495778d125870c },
		{ "ec_2101.ecl", 0xcaa687e50a835622 },
		{ "ec_2102.ecl", 0xc91a461022233b55 },
		{ "ec_2103.ecl", 0xea13f0c2610f133a },
		{ "ec_2104.ecl", 0xd5b99aef20b7c845 },
		{ "ec_2105.ecl", 0xec3787d5172c8f6f },
		{ "ec_2106.ecl", 0x57b1d4883a3cacf5 },
		{ "ec_21a.ecl", 0x2e59fb3da67a455f },
		{ "ec_21b.ecl", 0x9b8ccd629436a215 },
		{ "ec_21c.ecl", 0xe6c9b109d86f1813 },
		{ "ec_21d.ecl", 0x2e2b77316ca0e58e },
		{ "ec_2201.ecl", 0x99162b807c78486b },
		{ "ec_2202.ecl", 0x9ad3edeeea56e7c0 },
		{ "ec_2203.ecl", 0xacb0a916da22d73f },
		{ "ec_2204.ecl", 0x54d8f701bbad9cff },
		{ "ec_22b.ecl", 0xdb14446c58bb1fd2 },
		{ "ec_22d.ecl", 0x8ee549c85e98da85 },
		{ "effect.anm", 0x9298f0cea10036b3 },
		{ "enemy.anm", 0x24a649ef5ba7c5f8 },
		{ "enm01.anm", 0xbb3dd6c8cd3afc9c },
		{ "enm01b.anm", 0x48eaf5ad31689fa6 },
		{ "enm02.anm", 0x1a355c4ada1a304b },
		{ "enm03.anm", 0x24ca3256c13242c7 },
		{ "enm04.anm", 0x6d0dbb146e8aff00 },
		{ "enm05.anm", 0x8a3e9cbc543f8991 },
		{ "enm05b.anm", 0xc4a57a2f02620d26 },
		{ "enm06.anm", 0x7cf520f6f408cf66 },
		{ "enm07.anm", 0xa77c5378fa50a207 },
		{ "enm08.anm", 0xaf19cf47bb0792be },
		{ "enm09.anm", 0x7f2ab6a137d062df },
		{ "enm10.anm", 0x9259aee2ee9456d0 },
		{ "enm11.anm", 0x20a5e7630edb122a },
		{ "enm12.anm", 0x2938eb8836a3a84b },
		{ "enm13.anm", 0xd9dda9f70ec4be62 },
		{ "enm14.anm", 0xb82b7f78de28025a },
		{ "enm15.anm", 0x83b8fd7eadd61d08 },
		{ "enm16.anm", 0x20c2b614df26e024 },
		{ "enm16b.anm", 0x958c9563ab4d5312 },
		{ "enm17.anm", 0xa4a436bff6f761a5 },
		{ "enm18.anm", 0x8c92ee7ed0f394b },
		{ "enm19.anm", 0xa773b3d06280c919 },
		{ "enm20.anm", 0x53b2627b46e2feaf },
		{ "enm21.anm", 0xc673c378091f30d7 },
		{ "enm22.anm", 0x838202fe97ae2981 },
		{ "enm23.anm", 0xe27f4a5ee7db7218 },
		{ "enm24.anm", 0x7d1a99088df13ae4 },
		{ "enm25.anm", 0xa5b68d8d82542999 },
		{ "enm26.anm", 0x191505dab2687882 },
		{ "enm27.anm", 0xcefbe2990891395e },
		{ "enm28.anm", 0x188380323113880a },
		{ "enm29.anm", 0xd7e25420d491607a },
		{ "enm30.anm", 0x1422f1c1b78a3ac8 },
		{ "enm31.anm", 0x598c007cc5862a1c },
		{ "enm32.anm", 0xc8cf40810173405e },
		{ "enm33.anm", 0x2cd1b1f3483a63d6 },
		{ "enm34.anm", 0x10f787e7e017fb1e },
		{ "enm35.anm", 0x61850c6fccd341c6 },
		{ "enm36.anm", 0x2e68427ee3a4f45f },
		{ "enm37.anm", 0xcdab1ddbd81cb27a },
		{ "enm38.anm", 0xf241eb9c278ee4d2 },
		{ "enm39.anm", 0x4f30208ccdf2af93 },
		{ "enm40.anm", 0x87090311802c88b0 },
		{ "enm41.anm", 0xe40fe5d2420d7423 },
		{ "front.anm", 0xc8a117731b8f279e },
		{ "help.anm", 0x88998d585b076722 },
		{ "help_01.png", 0xdd4205b5533c7c83 },
		{ "help_02.png", 0xee9307ed68ff039b },
		{ "help_03.png", 0x1d191cd0ebe93805 },
		{ "help_04.png", 0xf7e58bd8b9baa288 },
		{ "help_05.png", 0x637e38d3feafe408 },
		{ "help_06.png", 0x7c3d67676a905fb9 },
		{ "help_07.png", 0xb78a0db67c2fdc82 },
		{ "help_08.png", 0xae16c5a3367992c0 },
		{ "help_09.png", 0x39798deda9a5acdb },
		{ "msg01.msg", 0xd06667664f03bcba },
		{ "msg02.msg", 0xc3a1c082257ff392 },
		{ "msg04.msg", 0x2eeda21426082a4c },
		{ "msg07.msg", 0x284119be9446ca97 },
		{ "msg08.msg", 0xd319c902044d2b6a },
		{ "msg11.msg", 0x5740e232b07255f2 },
		{ "msg14.msg", 0x314439ff37118117 },
		{ "msg22a.msg", 0xa61260a85a843d94 },
		{ "msg22b.msg", 0xb509d9ca244e7c3a },
		{ "msg22c.msg", 0x1afed41deced0125 },
		{ "msg22d.msg", 0x316612b49adf8a1e },
		{ "musiccmt.txt", 0xa73397e669dec518 },
		{ "notice.anm", 0xf3308127c3b5df33 },
		{ "notice_01.png", 0x3e084141eb6ce547 },
		{ "notice_02.png", 0x2cf4d0dd9c6cfc74 },
		{ "notice_03.png", 0x1b6ee0a20a4938cd },
		{ "notice_04.png", 0x3642e1c5050a5d70 },
		{ "notice_05.png", 0xa4d8cab47ec5379f },
		{ "notice_06.png", 0x5d0f33e91aedddf0 },
		{ "notice_07.png", 0xee721ec209a42550 },
		{ "notice_08.png", 0x4813140c5971d6e0 },
		{ "notice_09.png", 0xc23511e77a80ffb2 },
		{ "notice_10.png", 0xe284f0655a0e7f6b },
		{ "notice_11.png", 0x66f76a6279a91b1c },
		{ "notice_12.png", 0xf848428efd1006ad },
		{ "notice_13.png", 0xb97bf609cd7572de },
		{ "notice_14.png", 0x122c60ceceea10a5 },
		{ "notice_15.png", 0xb1bbc23241bc621c },
		{ "notice_16.png", 0x1ceb9ded926154f8 },
		{ "notice_17.png", 0x5699bfe13b78d177 },
		{ "notice_18.png", 0xe7d7a8f4a4cc6cff },
		{ "notice_19.png", 0x22b7149f811ed15e },
		{ "notice_20.png", 0x1b5a929e648bffb2 },
		{ "notice_21.png", 0x5b803a3f4e622440 },
		{ "notice_22.png", 0x308934a9f715eaad },
		{ "notice_23.png", 0x9890f69402a0a45 },
		{ "notice_24.png", 0x506d151669123216 },
		{ "notice_25.png", 0x29e41271f3d70d72 },
		{ "notice_26.png", 0x778c06c56281ef90 },
		{ "notice_27.png", 0x2021bbec74fbd2e6 },
		{ "notice_28.png", 0x82f5fc7f24b22371 },
		{ "notice_29.png", 0x6a3cfb7f97a59a42 },
		{ "notice_30.png", 0xac7150443a49c073 },
		{ "photo.anm", 0x679b1eee5698d334 },
		{ "pl00.anm", 0xfd6b3a4aa0930fb5 },
		{ "pl00.sht", 0xddccb9d67a61121 },
		{ "se_big.wav", 0xfb4ba3e45749198c },
		{ "se_bonus.wav", 0x333404301c4fc857 },
		{ "se_bonus2.wav", 0x368a5c8f1294dd5c },
		{ "se_bonus4.wav", 0xab65a052ae231a27 },
		{ "se_boon00.wav", 0x873bf73d48901836 },
		{ "se_boon01.wav", 0xb6e61cc0a10b2a47 },
		{ "se_cancel00.wav", 0x313cef4aa5505ad1 },
		{ "se_cardget.wav", 0x4f819b02eb7afab4 },
		{ "se_cat00.wav", 0x7495554f453ba1af },
		{ "se_cat01.wav", 0xdafdbe921b8a187a },
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
		{ "se_focusfix.wav", 0x52d7d93ec56cc756 },
		{ "se_focusfix2.wav", 0xf384b5ef0ce1e66 },
		{ "se_focusin.wav", 0xb12cc628c631ab01 },
		{ "se_focusrot.wav", 0x2fee41333e4f931a },
		{ "se_graze.wav", 0xe90a8e4fea7ce129 },
		{ "se_gun00.wav", 0xe1b3957205c72b7 },
		{ "se_heal.wav", 0xa7e6ed8a265161c5 },
		{ "se_invalid.wav", 0x429c1fa2823f6c10 },
		{ "se_item00.wav", 0x7b879e6f0f3274c7 },
		{ "se_item01.wav", 0x483a913fdaf532aa },
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
		{ "se_nice.wav", 0xa6ab57f4d6404778 },
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
		{ "se_release.wav", 0xdce9f51f983fdbbc },
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
		{ "se_warp_n.wav", 0x6f39952f415dc982 },
		{ "se_wolf.wav", 0x7608147ada84306 },
		{ "sig.anm", 0x7ee848a1f149ecaa },
		{ "text.anm", 0xa9c00e7321d2175d },
		{ "th165_0100a.ver", 0xa72074a003c7b7be },
		{ "thbgm.fmt", 0xcfac900712ff3d0b },
		{ "title.anm", 0x916a199cbb8300fe },
		{ "title_v.anm", 0x642aeeab01be5a01 },
		{ "titlemsg.txt", 0xdde421f39cf583de },
		{ "trophy.anm", 0x140ef3294c94be5a },
		{ "trophy.txt", 0x9d6007bc3ddc5b80 },
		{ "tutorial01.png", 0xdb414072ab5961c8 },
		{ "tutorial02.png", 0xb9d8572fcabd7acc },
		{ "tutorial03.png", 0xca6cc1515a651abe },
		{ "tutorial04.png", 0x886537fe47ee44d9 },
		{ "tutorial05.png", 0xc6d4065e61611bbd },
		{ "tutorial06.png", 0x51ba297744f2b86c },
		{ "tutorial07.png", 0xc5471c66cb704ece },
		{ "tutorial08.png", 0x4449146038b500b5 },
		{ "tutorial09.png", 0x13451b8270eb5f1e },
		{ "tutorial10.png", 0x1ebf039f330e3347 },
		{ "tutorial11.png", 0x8850dc9d512f8c26 },
		{ "tutorial12.png", 0xc0d62da193765374 },
		{ "tutorial13.png", 0x3cc341961c0885d2 },
		{ "tutorial14.png", 0x219e8b0d27bc538 },
		{ "tutorial15.png", 0x685101313c3be8f7 },
		{ "tutorial16.png", 0xac093cdef0455fc },
		{ "tutorial17.png", 0xe4a1382833074d23 },
		{ "tutorial18.png", 0x15528f8dac3863b3 },
		{ "tutorial19.png", 0x4e8721d9de036a20 },
		{ "tutorial20.png", 0x39222d57f4514c01 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th165";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th165-test.dat";

	public ArchiveTh165Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th165.dat")]
	public void ReadArchiveTh165(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.VD, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th165.dat", true)]
	public async Task ReadArchiveTh165Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.VD, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh165(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.VD, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.VD, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh165Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.VD, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.VD, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
