using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh19Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "abcard.anm", 0x62bba046c1187f91 },
		{ "ability.anm", 0x63ab13118ad5fabb },
		{ "ability.txt", 0xe6bbc19017bf2e38 },
		{ "abmenu.anm", 0xa982b94a9e566987 },
		{ "ascii.anm", 0x4d20e268a89788c0 },
		{ "ascii1280.anm", 0x8e4703141954e945 },
		{ "ascii_960.anm", 0x909cbaf9c7d1655b },
		{ "aura.anm", 0x30a51a150faa877d },
		{ "bullet.anm", 0x68597df76878e7d2 },
		{ "common.ecl", 0x44905e16bd045cf1 },
		{ "default.ecl", 0x14abbe3719f6079f },
		{ "ebg00.anm", 0x544acfd65b0998d3 },
		{ "ebg01.anm", 0x46a69fa1a9d5a4d7 },
		{ "ebg02.anm", 0xd98599651d097623 },
		{ "ebg03.anm", 0x931b859a95c7f29b },
		{ "ebg04.anm", 0x24163322a59cd208 },
		{ "ebg05.anm", 0x47ba44bfc85f8e4e },
		{ "ebg06.anm", 0x91d83d14a42b646d },
		{ "ebg07.anm", 0x6925cbc78f5ac067 },
		{ "ebg08.anm", 0x6ce8820efd39c499 },
		{ "effect.anm", 0xd74db60871820376 },
		{ "end00.anm", 0x83b80775bfd5aa8f },
		{ "end00.msg", 0x789b1bb65567282c },
		{ "end01.anm", 0xe8707fb026047422 },
		{ "end01.msg", 0x4e3a5e4232ccca1a },
		{ "end02.anm", 0xdd26548c4f5327e9 },
		{ "end02.msg", 0xf10d5a330a5a6fe7 },
		{ "end03.anm", 0x7105da0a3cf9ebdc },
		{ "end03.msg", 0xf70b47b0faecb8af },
		{ "end04.anm", 0x53df087930b9ab15 },
		{ "end04.msg", 0xeb17cd72a1207879 },
		{ "end05.anm", 0xdb14c8918831db1d },
		{ "end05.msg", 0x6d8b82e0ef60fc14 },
		{ "end06.anm", 0x9cf77818cd728147 },
		{ "end06.msg", 0x444ab4c1beb714c1 },
		{ "end07.anm", 0xb1df75a0792eafcc },
		{ "end07.msg", 0x382dbe423e4d6e94 },
		{ "end08.anm", 0x349559d991f726 },
		{ "end08.msg", 0xe9ba068508f2cc04 },
		{ "end09.anm", 0x920972ff3079b65e },
		{ "end09.msg", 0xdd705035948d59ae },
		{ "end10.anm", 0x11cb0c4c333c69d6 },
		{ "end10.msg", 0xd77a47faafbfd00c },
		{ "end11.anm", 0xffa977d930af8dd3 },
		{ "end11.msg", 0xdb3204c37d3cc54e },
		{ "end12.anm", 0xc408e4a25cf45a0d },
		{ "end12.msg", 0xbc36e5516bec622a },
		{ "end13.anm", 0x3f73693b525b5b61 },
		{ "end13.msg", 0xa0788c5c37ee471c },
		{ "end14.anm", 0x1995ab9bd85f1db4 },
		{ "end14.msg", 0x1dd4967a9faf43d2 },
		{ "end15.anm", 0x2afc9dcf26655951 },
		{ "end15.msg", 0xcf75f678d924e775 },
		{ "end16.anm", 0x92cb2295ebda2ecc },
		{ "end16.msg", 0xe45f54bad378d010 },
		{ "end17.anm", 0x6b00fef3ff12ed2d },
		{ "end17.msg", 0xc7a5036915712dfb },
		{ "end18.anm", 0x1e522ace3bf035cd },
		{ "end18.msg", 0xa7bbf09601d36212 },
		{ "enemy.anm", 0xc4ccb4ebe6cab737 },
		{ "front.anm", 0x47cde2a9d67f9c1f },
		{ "ghost.anm", 0xfa352d6030d56839 },
		{ "help.anm", 0xb7ad3bf18be80b7d },
		{ "help_01.png", 0xb6cbf6e705b8effa },
		{ "help_02.png", 0x3c749ba528788a8e },
		{ "help_03.png", 0xdad2bb61bd7a7bc },
		{ "help_04.png", 0x67c6df16e6cbc512 },
		{ "help_05.png", 0xfc5bcc6365e0bb44 },
		{ "help_06.png", 0x29157a3f099eb134 },
		{ "help_07.png", 0xf562364a1a1f5517 },
		{ "help_08.png", 0x9586ee8097a0ab30 },
		{ "help_09.png", 0x1ae504433d83b851 },
		{ "help_10.png", 0x8f1e7c88c2c18fa8 },
		{ "help_11.png", 0x54c60df1f2e978af },
		{ "help_12.png", 0xb9eb086e445448c1 },
		{ "help_13.png", 0xfd43d95fabff083b },
		{ "musiccmt.txt", 0x4f1798a18e5a8e04 },
		{ "network.anm", 0x3f37daf77b5ee40f },
		{ "notice.anm", 0x459ce70f2ef9064e },
		{ "notice_01.png", 0xb7eea3099fef1265 },
		{ "pl00.anm", 0x94e0a146b697427e },
		{ "pl00.ecl", 0xa031ab630446ef7 },
		{ "pl00.sht", 0xe6982ca01f008f93 },
		{ "pl00b.anm", 0xf9f2e6c42edccb93 },
		{ "pl00f.anm", 0xbdd4004b8b10f6b7 },
		{ "pl00st.msg", 0x6c9b61807cf9ab26 },
		{ "pl00vs.msg", 0xed8771edd0afb36 },
		{ "pl01.anm", 0x84ebc67b5555c721 },
		{ "pl01.ecl", 0x35112fa4a62cb9f5 },
		{ "pl01.sht", 0x6d9038c8432c1898 },
		{ "pl01b.anm", 0x336a17182e67ebdd },
		{ "pl01f.anm", 0x89c110a229007f8f },
		{ "pl01st.msg", 0xda16d936ce564d42 },
		{ "pl01vs.msg", 0xaf7b360bc1c68848 },
		{ "pl02.anm", 0xfd8faf0578c04101 },
		{ "pl02.ecl", 0x31f878bb56052c27 },
		{ "pl02.sht", 0xb65a6cc084d0deb8 },
		{ "pl02b.anm", 0xa348636ecf7530ad },
		{ "pl02f.anm", 0x9ce9a18dcf015f7c },
		{ "pl02st.msg", 0x3c75062b44766d0e },
		{ "pl02vs.msg", 0x2c2fde859bfccf1f },
		{ "pl03.anm", 0xa4e704c31d6cc838 },
		{ "pl03.ecl", 0x8973aedc6537f7e2 },
		{ "pl03.sht", 0xfb28b4c37d703a6f },
		{ "pl03b.anm", 0x9a9c51ef40237f9f },
		{ "pl03f.anm", 0x482ed2e31ea48a60 },
		{ "pl03st.msg", 0x5a2488f8a187c304 },
		{ "pl03vs.msg", 0x13ff1f22753a15cf },
		{ "pl04.anm", 0xb46639a79a82b182 },
		{ "pl04.ecl", 0x1851448a9f859cf7 },
		{ "pl04.sht", 0x1e57f9e14c579824 },
		{ "pl04b.anm", 0x7176e074012c426 },
		{ "pl04f.anm", 0xb217da9d55d39d7e },
		{ "pl04st.msg", 0x6ebea840e547c687 },
		{ "pl04vs.msg", 0xa69d7e727037d0b0 },
		{ "pl05.anm", 0x70ad45aa8cc921e4 },
		{ "pl05.ecl", 0x393ba6e130f449c7 },
		{ "pl05.sht", 0x74f12b4850df7e6d },
		{ "pl05b.anm", 0x5917f4ff75b3b8b3 },
		{ "pl05f.anm", 0x30268d16594f9b49 },
		{ "pl05st.msg", 0x4bdd827bab9ef1e1 },
		{ "pl05vs.msg", 0xc36a7d2402515e2e },
		{ "pl06.anm", 0x989f8b9c2f34fa41 },
		{ "pl06.ecl", 0xb0c5956317c943d },
		{ "pl06.sht", 0x24794137d420d3e8 },
		{ "pl06b.anm", 0xf2ea343ba0bb8860 },
		{ "pl06f.anm", 0x8e253e6f78b6e1bc },
		{ "pl06st.msg", 0xbe378ad3a0143b96 },
		{ "pl06vs.msg", 0x21f0b2880e334a02 },
		{ "pl07.anm", 0x2c5fe8cdeacbdcb1 },
		{ "pl07.ecl", 0xe31c92e0300ba6e1 },
		{ "pl07.sht", 0xffc1f59ab4765174 },
		{ "pl07b.anm", 0xb72a4daddc5243fe },
		{ "pl07f.anm", 0x264cf507468ab9e7 },
		{ "pl07st.msg", 0x25aefa2b080e8681 },
		{ "pl07vs.msg", 0x202050790a8ed295 },
		{ "pl08.anm", 0xaafcb9028533f3a1 },
		{ "pl08.ecl", 0x435a16f91568b06c },
		{ "pl08.sht", 0x9c5185e5037e53c8 },
		{ "pl08b.anm", 0xe70d3616e24f71f5 },
		{ "pl08f.anm", 0xea08c572da4ec076 },
		{ "pl08st.msg", 0x768cdcb1a86c3a65 },
		{ "pl08vs.msg", 0x62c2b3bc9502db31 },
		{ "pl09.anm", 0x70d08ad6e0d752ef },
		{ "pl09.ecl", 0xce8f840f92f7f08d },
		{ "pl09.sht", 0xaaee4fb6137b22e3 },
		{ "pl09b.anm", 0xa46da388e0880aec },
		{ "pl09f.anm", 0x3f688cdd0967c1a6 },
		{ "pl09st.msg", 0x2895fd4cd30c5cba },
		{ "pl09vs.msg", 0xae9e5c701b67d667 },
		{ "pl10.anm", 0xa3a136187f93f8de },
		{ "pl10.ecl", 0x16d7baac14aca62 },
		{ "pl10.sht", 0xe8aecc17500409dc },
		{ "pl10b.anm", 0x23d7164afe4d5f6c },
		{ "pl10f.anm", 0x1cdb108cb3755ae0 },
		{ "pl10st.msg", 0x9a24527cf047c101 },
		{ "pl10vs.msg", 0x9f6b677d1cba0289 },
		{ "pl11.anm", 0x3500f6dfda31d5bf },
		{ "pl11.ecl", 0xb47fa7ff43784b0b },
		{ "pl11.sht", 0x4ce55ec02f72b825 },
		{ "pl11b.anm", 0x26ce21930bbc4a2 },
		{ "pl11f.anm", 0x43ae00195aa25531 },
		{ "pl11st.msg", 0x27e60440866e703b },
		{ "pl11vs.msg", 0xaacd1f805701fdfc },
		{ "pl12.anm", 0x4d7d0de59543fad5 },
		{ "pl12.ecl", 0x6bea615ab59e8d28 },
		{ "pl12.sht", 0xab9d8c6a92e77ab8 },
		{ "pl12b.anm", 0x5b8cb806d600e06f },
		{ "pl12f.anm", 0x80957c3e0185f4e2 },
		{ "pl12st.msg", 0xc25bd0756a354b9f },
		{ "pl12vs.msg", 0x109c19a959754be8 },
		{ "pl13.anm", 0xe9b42c4af968c5f8 },
		{ "pl13.ecl", 0x9224b376662deb9c },
		{ "pl13.sht", 0x24df5573f4020caa },
		{ "pl13b.anm", 0x81e64523730f0687 },
		{ "pl13f.anm", 0x2766bc90616bdd25 },
		{ "pl13st.msg", 0xfa527e419b2a66e8 },
		{ "pl13vs.msg", 0x1e11861479935a53 },
		{ "pl14.anm", 0x79fcb72fdc6b5b0c },
		{ "pl14.ecl", 0x5a5bed0f29f96aa1 },
		{ "pl14.sht", 0x5595167c0809b87a },
		{ "pl14b.anm", 0x40b66080d09ed9e8 },
		{ "pl14f.anm", 0x8b4c1591ab81c73d },
		{ "pl14st.msg", 0x9efe433e5bd62c7a },
		{ "pl14vs.msg", 0x1f44f912234ea9b4 },
		{ "pl15.anm", 0xef60f08a0fa7abf },
		{ "pl15.ecl", 0x8349666bd87e1ce9 },
		{ "pl15.sht", 0x630fb34c129e5963 },
		{ "pl15b.anm", 0x9b1f38d119904c83 },
		{ "pl15f.anm", 0x2f11ef169fba33f4 },
		{ "pl15st.msg", 0xda2db79c1bd8f5d6 },
		{ "pl15vs.msg", 0x16dea10a19c03c91 },
		{ "pl16.anm", 0x6e4f5497ee52cb13 },
		{ "pl16.ecl", 0x80dcb66f01af02b5 },
		{ "pl16.sht", 0x60269361c61cbd25 },
		{ "pl16b.anm", 0x3419b97619281568 },
		{ "pl16f.anm", 0xf86c92d0b288ee26 },
		{ "pl16st.msg", 0x862400c6a9e010d4 },
		{ "pl16vs.msg", 0x9d94e0e4b83a035 },
		{ "pl17.anm", 0xb919320fbf5d2ff6 },
		{ "pl17.ecl", 0xf81d7a5ce6a69298 },
		{ "pl17.sht", 0x52083ec7b7913400 },
		{ "pl17b.anm", 0x16fdddc384085ad7 },
		{ "pl17f.anm", 0x7a97e5be150cafbc },
		{ "pl17st.msg", 0xb990837b00edef33 },
		{ "pl17vs.msg", 0xf568ca6840c83f21 },
		{ "pl18.anm", 0x1a309edfe4b8b178 },
		{ "pl18.ecl", 0x1fd321f3def59ecc },
		{ "pl18.sht", 0xab601b0be7adf61e },
		{ "pl18b.anm", 0xcf38f2a2fcd5ef3 },
		{ "pl18f.anm", 0xbce16fbf71cc9e24 },
		{ "pl18st.msg", 0x93472ceec96bdb93 },
		{ "pl18vs.msg", 0x40c216186e97ec27 },
		{ "screenswitch.anm", 0xf1d7ab44f99cbe97 },
		{ "se_big.wav", 0xfb4ba3e45749198c },
		{ "se_bonus.wav", 0x333404301c4fc857 },
		{ "se_bonus2.wav", 0x368a5c8f1294dd5c },
		{ "se_bonus4.wav", 0xab65a052ae231a27 },
		{ "se_boon00.wav", 0x873bf73d48901836 },
		{ "se_boon01.wav", 0xb6e61cc0a10b2a47 },
		{ "se_cancel00.wav", 0x313cef4aa5505ad1 },
		{ "se_cardget.wav", 0x4f819b02eb7afab4 },
		{ "se_cat00.wav", 0x7495554f453ba1af },
		{ "se_ch00.wav", 0x177cbb5247feaf37 },
		{ "se_ch01.wav", 0x48999fd2fcd647da },
		{ "se_ch02.wav", 0x4514aa1d8424c7e2 },
		{ "se_ch03.wav", 0x336f51977bc49eca },
		{ "se_changeitem.wav", 0x6aec872d94ad63df },
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
		{ "se_slash.wav", 0x47035821cb00e36d },
		{ "se_tan00.wav", 0xa102dc9427328bb2 },
		{ "se_tan01.wav", 0xc33ecd338cbc6caf },
		{ "se_tan02.wav", 0x5f00638b5531716 },
		{ "se_tan03.wav", 0x73387ed64c1c8f3a },
		{ "se_timeout.wav", 0x7759a1efbe58daf4 },
		{ "se_timeout2.wav", 0x82699d4c0c0a2be5 },
		{ "se_trophy.wav", 0x1cf3838471ae12d2 },
		{ "se_warpl.wav", 0x3f46b12f94b42d43 },
		{ "se_warpr.wav", 0x8940e22b4364b080 },
		{ "se_wolf.wav", 0x7608147ada84306 },
		{ "sig.anm", 0x9f17c2b0e6023053 },
		{ "staff.anm", 0xc1fadb6cd29dd21e },
		{ "staff.msg", 0x63144a15be4cf6d8 },
		{ "text.anm", 0xd38b127eff2fbba3 },
		{ "th19_100a.ver", 0xc0553289ccc4873c },
		{ "thbgm.fmt", 0xf0fedce323b47cd5 },
		{ "title.anm", 0x341ff930af2562ab },
		{ "title_v.anm", 0xa76be12559605e4f },
		{ "trophy.anm", 0x1aa69c13c4fd5610 },
		{ "trophy.txt", 0x4731e4897372a6f },
		{ "wave01.ecl", 0xf535057b6dbec912 },
		{ "wave01f.ecl", 0x5cbc69eb2a64c388 },
		{ "wave02.ecl", 0xf586bc2a37850cd8 },
		{ "wave02f.ecl", 0x26f67ec3485b7261 },
		{ "wave03.ecl", 0x2b21e2fc913f0529 },
		{ "wave03f.ecl", 0xfda1787786f6f376 },
		{ "wave04.ecl", 0xb4618c0fdae5ecfb },
		{ "wave04f.ecl", 0xcfb782fa673d5b96 },
		{ "wave05.ecl", 0xdfbfdade75b06264 },
		{ "wave05f.ecl", 0x3ed4b1930c8a409d },
		{ "wave06.ecl", 0xd8927c3361170368 },
		{ "wave06f.ecl", 0xed40d9cee3c44496 },
		{ "wave07.ecl", 0xd0569381216ad261 },
		{ "wave07f.ecl", 0x9e43e88bcd1c5e67 },
		{ "wave08.ecl", 0xd6fcb96bfb82074d },
		{ "wave08f.ecl", 0x479adf76cbaaa99 },
		{ "wave09.ecl", 0xc8a28bc4de163ad2 },
		{ "wave09f.ecl", 0x827015454f2acb6a },
		{ "wave10.ecl", 0xa45af5f52d345387 },
		{ "wave10f.ecl", 0xf9986d985b894a66 },
		{ "wave11.ecl", 0xc0c9722698e854dc },
		{ "wave11f.ecl", 0x8eea50a7ac4580fb },
		{ "world01.anm", 0xccd2ecbd84bcd805 },
		{ "world01.std", 0x23adb40fcbc63b56 },
		{ "world02.anm", 0xecc5be24496d321e },
		{ "world02.std", 0xbccf34a85a9f08fe },
		{ "world03.anm", 0x42d71d3c252215 },
		{ "world03.std", 0x23af3ba4cbf0d4c },
		{ "world04.anm", 0xd01e39adf8224741 },
		{ "world04.std", 0xec538e1e7c487282 },
		{ "world05.anm", 0x9679f724ffcffb60 },
		{ "world05.std", 0xd002d5cefa486212 },
		{ "world06.anm", 0x72a6afd17d60cd00 },
		{ "world06.std", 0x9063545f40e192de },
		{ "world07.anm", 0x505c95b933cd8f0c },
		{ "world07.std", 0xaaa558442628d790 },
		{ "world08.anm", 0x33f338f04bc028bb },
		{ "world08.std", 0x25d9ae60f72d69e6 },
		{ "world09.anm", 0xe3a387d3bc110017 },
		{ "world09.std", 0xdc31d01f360e11df },
		{ "world10.anm", 0x6f9831326fc7fdde },
		{ "world10.std", 0x1e691774e59e357f },
		{ "world11.anm", 0x2ba42d29a761caea },
		{ "world11.std", 0x87cdaf76078d2a89 },
		{ "world12.anm", 0xa48dc228fb173a69 },
		{ "world12.std", 0xc5d3aaea72ebb3ca },
		{ "world13.anm", 0x29dbb4856f305a6e },
		{ "world13.std", 0x379492478168c1a7 },
		{ "world14.anm", 0xce292d83a7dac29 },
		{ "world14.std", 0xcf380c2462704188 },
		{ "world15.anm", 0x27cffcc42135c92e },
		{ "world15.std", 0xedd5cb9635afc646 },
		{ "world16.anm", 0xd16db3a3e82d1499 },
		{ "world16.std", 0x42fec87942feaa19 },
		{ "world17.anm", 0x322636fd83145b64 },
		{ "world17.std", 0x66cc0134df058727 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th19";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th19-test.dat";

	public ArchiveTh19Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th19.dat")]
	public void ReadArchiveTh19(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.UDoALG, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th19.dat", true)]
	public async Task ReadArchiveTh19Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.UDoALG, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh19(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.UDoALG, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.UDoALG, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh19Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.UDoALG, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.UDoALG, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
