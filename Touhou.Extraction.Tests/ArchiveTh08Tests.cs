using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh08Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "ascii.anm", 0x3c822bee2581f440 },
		{ "capture.anm", 0xbe53d400fc1b732b },
		{ "demorpy0.rpy", 0xed6a6511d042bcfc },
		{ "demorpy1.rpy", 0x6588708543b5d554 },
		{ "demorpy2.rpy", 0x246d5198c0861961 },
		{ "demorpy3.rpy", 0x30d125d3f25a44a },
		{ "ecldata1.ecl", 0xa4bd12324697c9dd },
		{ "ecldata1sp.ecl", 0x3ebf32a5c457bc85 },
		{ "ecldata2.ecl", 0x4bba343cbf87dc },
		{ "ecldata2sp.ecl", 0x205a2d540a60f16e },
		{ "ecldata3.ecl", 0x2a85aa431bd37719 },
		{ "ecldata3sp.ecl", 0x24e77f9025dce03f },
		{ "ecldata4a.ecl", 0x9c9fd5c9b8a62226 },
		{ "ecldata4asp.ecl", 0x255a5dc9614f56bc },
		{ "ecldata4b.ecl", 0xda60d78a7b8b035f },
		{ "ecldata4bsp.ecl", 0xde268e67603bb97e },
		{ "ecldata5.ecl", 0x3e86175e22f926ca },
		{ "ecldata5sp.ecl", 0x8547175826e154f7 },
		{ "ecldata6.ecl", 0x4c6b289bf06228b5 },
		{ "ecldata6sp.ecl", 0xd4c3319f3b04ac68 },
		{ "ecldata7.ecl", 0x1cc80b75428de342 },
		{ "ecldata7sp.ecl", 0x2f35ea60d2a23038 },
		{ "ecldata8.ecl", 0x25bcec175e605e7e },
		{ "ecldata8sp.ecl", 0xd0b5ef0f6d12aa98 },
		{ "ecldata_al.ecl", 0xf1b0fda26ac84f0d },
		{ "ecldata_rm.ecl", 0x3d67206ce6f641aa },
		{ "ecldata_sk.ecl", 0xe53662ba7de9e4fb },
		{ "ecldata_yk.ecl", 0x655fdf0f05581def },
		{ "ecldata_ym.ecl", 0x151a85e26a1a37e7 },
		{ "ecldata_yy.ecl", 0x6cc3b13c57d7951c },
		{ "eff01.anm", 0x4f843e1d4fb76808 },
		{ "eff02.anm", 0x9b603b544b376f42 },
		{ "eff03.anm", 0x792ba184eb98b5da },
		{ "eff04a.anm", 0xdecb72b3b7c5bdee },
		{ "eff04b.anm", 0xde60ace89e6c6139 },
		{ "eff05.anm", 0x1274686f576065c7 },
		{ "eff06.anm", 0x15fdf3f328f55883 },
		{ "eff07.anm", 0x2d1d1989a4823353 },
		{ "eff08.anm", 0x23e1af957d9fd2ff },
		{ "eff09al.anm", 0xd4ee165af96a581 },
		{ "eff09rm.anm", 0xf1acd963e37f7f8b },
		{ "eff09sk.anm", 0x54e764d3bb73260 },
		{ "eff09yk.anm", 0x2f8bcff13d9786be },
		{ "eff09ym.anm", 0x84c203f0bb8655d },
		{ "eff09yy.anm", 0x3ffff7de27f6167e },
		{ "end00.jpg", 0x99cec79444ac3d89 },
		{ "end00a.end", 0x24171f9ce463eee4 },
		{ "end00b.end", 0xf626233375d4d38a },
		{ "end00b.jpg", 0x44d4e08582876af5 },
		{ "end00b0.jpg", 0x9857d30efb5aafd },
		{ "end00b1.jpg", 0xcc5db05ce6a8fc63 },
		{ "end00c.end", 0xc8a9df3770693ad },
		{ "end00c0.jpg", 0x1f9e2e109655e455 },
		{ "end00c1.jpg", 0x60f43d2a4a363934 },
		{ "end00c2.jpg", 0xbb57320f83d582ac },
		{ "end01.jpg", 0x6d878127975ac282 },
		{ "end01a.end", 0xda9226c5115b1300 },
		{ "end01b.end", 0x8759dd53e5a17950 },
		{ "end01b0.jpg", 0x243922fdad3729a6 },
		{ "end01b1.jpg", 0x3a8edfb99e4e5f5c },
		{ "end01c.end", 0x50a892d862768fac },
		{ "end01c.jpg", 0x85b1299b156d19d7 },
		{ "end01c0.jpg", 0x3a7a22bd59bf2300 },
		{ "end01c1.jpg", 0x273c787d5a808fc3 },
		{ "end01c2.jpg", 0xe7769046fa3fa338 },
		{ "end02.jpg", 0x43330da004729816 },
		{ "end02a.end", 0x3f90d798b3120f93 },
		{ "end02b.end", 0x943828194309d581 },
		{ "end02b.jpg", 0xfd8db4eabb5ba26e },
		{ "end02b0.jpg", 0x672b812ce3e9ba49 },
		{ "end02b1.jpg", 0xda6a1be34c87381c },
		{ "end02c.end", 0x14efc5cc2e44d059 },
		{ "end02c0.jpg", 0xf53b7b49903447a4 },
		{ "end02c1.jpg", 0xf72e6f38110ef09c },
		{ "end02c2.jpg", 0x7c5125236457261b },
		{ "end03.jpg", 0x251fb1a8f507dea2 },
		{ "end03a.end", 0x6ace653dfaab2845 },
		{ "end03b.end", 0x13d081c2477b2ba8 },
		{ "end03b.jpg", 0xbf2aba89610c1308 },
		{ "end03b0.jpg", 0x3fa817f65c57bd32 },
		{ "end03b1.jpg", 0x6565c7d44a437929 },
		{ "end03c.end", 0xca4a199ebafcef65 },
		{ "end03c0.jpg", 0x7e65151ac49f60ca },
		{ "end03c1.jpg", 0x5c2e9996a7c1e6c9 },
		{ "end03c2.jpg", 0xe9210d4e72ded2c0 },
		{ "enemy.anm", 0xd914ec7d6bf798df },
		{ "etama.anm", 0xaa199bc1e57b8747 },
		{ "face_al00.anm", 0x149ea96d63ccb661 },
		{ "face_alsp.anm", 0x1cd61cd648470404 },
		{ "face_cdbg.anm", 0xf3949af6dc536f84 },
		{ "face_mr00.anm", 0x16a67eb2bcd82829 },
		{ "face_rm00.anm", 0x2ec52e41f0bb0a3d },
		{ "face_rs00.anm", 0xd70b67a9fff0e697 },
		{ "face_rssp.anm", 0x5b929eb3337e317d },
		{ "face_sk00.anm", 0xfca9300e2bd0e743 },
		{ "face_sksp.anm", 0x53d216e84021ce8 },
		{ "face_st01.anm", 0x5a85693001f830ce },
		{ "face_st01sp.anm", 0xa02af5074de44b2 },
		{ "face_st02.anm", 0x791509051e5b6259 },
		{ "face_st02sp.anm", 0xe647a27e57f53214 },
		{ "face_st03.anm", 0xfbc03aec53e02b22 },
		{ "face_st03sp.anm", 0xced0341f4e1fd7f1 },
		{ "face_st04a.anm", 0x78185ec17ee70af1 },
		{ "face_st04asp.anm", 0xafda7439bfba7a5d },
		{ "face_st04b.anm", 0x72238fdd5aef52c },
		{ "face_st04bsp.anm", 0xc0a078d3d88a3436 },
		{ "face_st05.anm", 0xa2685d2e62c26f37 },
		{ "face_st05b.anm", 0x5f8a651032de4529 },
		{ "face_st05msp.anm", 0x31027f19818129e1 },
		{ "face_st05sp.anm", 0x13b16d5564699ddd },
		{ "face_st06.anm", 0xa53b63041a9b3a05 },
		{ "face_st06sp.anm", 0x7bad8ae0903ccffb },
		{ "face_st07.anm", 0x126298a11280b269 },
		{ "face_st07sp.anm", 0xce7bf3ddf5feaed0 },
		{ "face_st08.anm", 0x6304b7028968de08 },
		{ "face_st08m.anm", 0x4da4db1186e73ab0 },
		{ "face_st08msp.anm", 0xa4f5728595b07715 },
		{ "face_st08sp.anm", 0x5674a01c4ed8068f },
		{ "face_yk00.anm", 0x739c275d514546e0 },
		{ "face_yksp.anm", 0x692327355441c7f6 },
		{ "face_ym00.anm", 0x8e491d122aa69d16 },
		{ "face_ymsp.anm", 0x2a9c8c8045fc7846 },
		{ "face_yy00.anm", 0xb84891827e666b4a },
		{ "face_yysp.anm", 0x1a5434e9c71a33f8 },
		{ "front.anm", 0x479db4e8fa71daa3 },
		{ "info00.jpg", 0xdfb7f2bc5398e798 },
		{ "info01.jpg", 0xf70f3387ec125c15 },
		{ "info02.jpg", 0x13e34f8e08a0f8a9 },
		{ "init.mid", 0x615b08e868387ca9 },
		{ "loading00.anm", 0xdfd0e9def5788174 },
		{ "loading00a.anm", 0xebeae1b2267af910 },
		{ "loading00h.anm", 0x2a471498f4fc58d6 },
		{ "loading01.anm", 0x85f805e4da9c6f04 },
		{ "loading01a.anm", 0xf7599e7f0ded6561 },
		{ "loading01h.anm", 0x7bab8ae12cb45020 },
		{ "loading02.anm", 0x46d06e86037279e5 },
		{ "loading02a.anm", 0xea054b19feab3a31 },
		{ "loading02h.anm", 0x335a843026f42d4e },
		{ "loading03.anm", 0xadfcd89e14fed7f4 },
		{ "loading03a.anm", 0x2831a98d5ec7111b },
		{ "loading03h.anm", 0xdc7cc98a783e0f6 },
		{ "msg1a.dat", 0xa0ae50b05dce0184 },
		{ "msg1b.dat", 0x7bde1f22fc5f0c01 },
		{ "msg1c.dat", 0xf6139a5e3d9b39d2 },
		{ "msg1d.dat", 0x1a127982d8130b0c },
		{ "msg2a.dat", 0xe1e56c0d9be627b6 },
		{ "msg2b.dat", 0x6da0600c5da19276 },
		{ "msg2c.dat", 0x2ef2d436e3fc20e3 },
		{ "msg2d.dat", 0xe584c86341cdf08b },
		{ "msg3a.dat", 0x8e5fd79ca2c3e388 },
		{ "msg3b.dat", 0xd45607d954ec7398 },
		{ "msg3c.dat", 0xef27e61013911dc6 },
		{ "msg3d.dat", 0x3ea66cc56a3bea5b },
		{ "msg4ab.dat", 0x18c417e85f66ac5 },
		{ "msg4ac.dat", 0xea92607d5904dc1b },
		{ "msg4ba.dat", 0x8a3afe111cb40c2e },
		{ "msg4bd.dat", 0xd98e37dbbfdb3364 },
		{ "msg4dm.dat", 0x441723cf39fd9217 },
		{ "msg5a.dat", 0xd86e9a9d0f7ab7b },
		{ "msg5b.dat", 0xe0901c12338c8bde },
		{ "msg5c.dat", 0x4cf902de36dc7da8 },
		{ "msg5d.dat", 0xbce3cf667348966 },
		{ "msg6a.dat", 0xe291ba4fb4ef43f7 },
		{ "msg6b.dat", 0x4a6a5cd55d38b56e },
		{ "msg6c.dat", 0x43c17f6eb190500d },
		{ "msg6d.dat", 0xef6f046a0ed2bf92 },
		{ "msg7a.dat", 0xee765468dc72e62 },
		{ "msg7b.dat", 0x65fd4b8eb0a34164 },
		{ "msg7c.dat", 0x8988ab4b8ba44dc },
		{ "msg7d.dat", 0x68f627d4d6a141dd },
		{ "msg8a.dat", 0x3cd3bf99c60a991d },
		{ "msg8b.dat", 0x204fd5ed14f2e792 },
		{ "msg8c.dat", 0x1b64ce2e46925ab },
		{ "msg8d.dat", 0x14382aeb830b0001 },
		{ "music.jpg", 0xcc8582227385ae93 },
		{ "music00.anm", 0xc545b0087238bf18 },
		{ "musiccmt.txt", 0x3170890ab41ba4ae },
		{ "nowloading.anm", 0x44999248c52d6e6 },
		{ "phantasm.jpg", 0xe939da7a201641eb },
		{ "player00.anm", 0x9170884e9976cd68 },
		{ "player01.anm", 0x42102ca118918f2d },
		{ "player02.anm", 0xf0d6ac4986c496f7 },
		{ "player03.anm", 0x7675af414f99c42 },
		{ "ply00a.sht", 0xe01d664be1e05085 },
		{ "ply00as.sht", 0x7082b833141d2a04 },
		{ "ply01a.sht", 0x85aad9c0bc2e2435 },
		{ "ply01as.sht", 0x7c3dada0740e2305 },
		{ "ply02a.sht", 0x64dfb556702b33fe },
		{ "ply02as.sht", 0x71637003d77c417b },
		{ "ply03a.sht", 0xa74bf6b9914d0433 },
		{ "ply03as.sht", 0xeb6be4194cb167ef },
		{ "result.jpg", 0x487eaa09b5436e38 },
		{ "result00.anm", 0x1e42c94c12f7106b },
		{ "resulttext.anm", 0xb978ec31b038a886 },
		{ "se_bonus.wav", 0x333404301c4fc857 },
		{ "se_bonus2.wav", 0x368a5c8f1294dd5c },
		{ "se_border.wav", 0x27471485eab0fd5a },
		{ "se_cancel00.wav", 0x313cef4aa5505ad1 },
		{ "se_cardget.wav", 0x4f819b02eb7afab4 },
		{ "se_cat00.wav", 0x7495554f453ba1af },
		{ "se_damage00.wav", 0x6600b8e43570fa8d },
		{ "se_damage01.wav", 0x4c72fdc517387fff },
		{ "se_enep00.wav", 0x699b01dc8d1eb6c6 },
		{ "se_enep01.wav", 0xa9512fb3297e92ea },
		{ "se_extend.wav", 0x80fc940fa2f0ed20 },
		{ "se_graze.wav", 0xe90a8e4fea7ce129 },
		{ "se_gun00.wav", 0xe1b3957205c72b7 },
		{ "se_invalid.wav", 0x429c1fa2823f6c10 },
		{ "se_item00.wav", 0x7b879e6f0f3274c7 },
		{ "se_item01.wav", 0x1c2602eb1a15bb36 },
		{ "se_kira00.wav", 0x14e39aa59675931b },
		{ "se_kira01.wav", 0x11f6fedced1fea00 },
		{ "se_kira02.wav", 0xa608ce966fae0898 },
		{ "se_lazer00.wav", 0x9c010a6fd5aadc25 },
		{ "se_lazer01.wav", 0x5ef637b48e701396 },
		{ "se_nep00.wav", 0xda4e266afe0b1370 },
		{ "se_ok00.wav", 0x211c94183d368af0 },
		{ "se_ophide.wav", 0x5c10cc0bc4a8ab51 },
		{ "se_opshow.wav", 0x6b105d3c0d6a6c80 },
		{ "se_option.wav", 0xadabeccdcebc65c3 },
		{ "se_pause.wav", 0xf1ec61b783b86910 },
		{ "se_pldead00.wav", 0x1d3315cbdfbab9e9 },
		{ "se_plst00.wav", 0xed6645fe2e41e0c },
		{ "se_power0.wav", 0xdc3657c81ae4f0c3 },
		{ "se_power1.wav", 0xd89bb75d35046555 },
		{ "se_powerup.wav", 0x18d8eb75d98e6104 },
		{ "se_select00.wav", 0xe8a68aca740db942 },
		{ "se_slash.wav", 0x47035821cb00e36d },
		{ "se_tan00.wav", 0xa102dc9427328bb2 },
		{ "se_tan01.wav", 0xc33ecd338cbc6caf },
		{ "se_tan02.wav", 0x5f00638b5531716 },
		{ "se_timeout.wav", 0x7759a1efbe58daf4 },
		{ "se_timeout2.wav", 0x82699d4c0c0a2be5 },
		{ "select00.png", 0x8de37a09d77a5d4d },
		{ "staff00.end", 0xb4875106016f39ae },
		{ "staff00.jpg", 0xd74f4bc7701ec51a },
		{ "staff00b.end", 0x51e0e6368c561eb7 },
		{ "staff00b.jpg", 0xfa5cc9f0e8d3b984 },
		{ "staff01.anm", 0xa3344202c7f8cc2d },
		{ "stage1.std", 0x91ec84e73d9033d8 },
		{ "stage1_s.std", 0xf0eddd1a772f70df },
		{ "stage2.std", 0xd2b2a2a9f0902113 },
		{ "stage2_s.std", 0x5120e09a76108039 },
		{ "stage3.std", 0x92d34260c35c4fb1 },
		{ "stage3_s.std", 0xd30920a9a56457a1 },
		{ "stage4a.std", 0x4fa596a35cd05e91 },
		{ "stage4a_s.std", 0x61cf58818a97ea24 },
		{ "stage4b.std", 0x60c59f6d41e71d6 },
		{ "stage4b_s.std", 0x61cf58818a97ea24 },
		{ "stage5.std", 0xc01fc634cdff368a },
		{ "stage5_s.std", 0x2aae245072147dff },
		{ "stage6.std", 0x8d74e7ca15a362a8 },
		{ "stage6_s.std", 0x88146e07d2bcf611 },
		{ "stage7.std", 0xc9e4f4b095c73a43 },
		{ "stage7_s.std", 0x73fd4946756367b4 },
		{ "stage8.std", 0x4f28dc24c61fa364 },
		{ "stage8_s.std", 0xa5320528807a48d2 },
		{ "stg1bg.anm", 0x6ca2073d8ef5af4f },
		{ "stg1enm.anm", 0xbbf688b8a5567b2e },
		{ "stg1txt.anm", 0x84a5785c3e9ebddb },
		{ "stg2bg.anm", 0x46ae7722f04ad64d },
		{ "stg2enm.anm", 0xe701f5c20ab38fd4 },
		{ "stg2txt.anm", 0xc78769e6e2d3b517 },
		{ "stg3bg.anm", 0x618632175d5f79d7 },
		{ "stg3enm.anm", 0xe4a267d2fa791244 },
		{ "stg3txt.anm", 0x4b4f1dc2658ff074 },
		{ "stg4abg.anm", 0x1f4453870a530260 },
		{ "stg4aenm.anm", 0x72a08cd9e3babf15 },
		{ "stg4atxt.anm", 0xa59a92bab4429703 },
		{ "stg4benm.anm", 0xb1391c6d04b437e0 },
		{ "stg4btxt.anm", 0xea3e1c4282dabb0a },
		{ "stg5bg.anm", 0xa47c9456133098b },
		{ "stg5enm.anm", 0x9f8c408c5a61e75b },
		{ "stg5txt.anm", 0xfbf0484f3297bf05 },
		{ "stg6bg.anm", 0xeda8486dd2d48052 },
		{ "stg6enm.anm", 0x481b360322677e30 },
		{ "stg6txt.anm", 0x57ab6451f5996f91 },
		{ "stg7bg.anm", 0xd9e10d28309f6ac7 },
		{ "stg7enm.anm", 0x2905a57128f151c2 },
		{ "stg7txt.anm", 0x8869d89900ba8d57 },
		{ "stg8bg.anm", 0xbebdab590848668e },
		{ "stg8enm.anm", 0x16eb7fa1743381f3 },
		{ "stg8txt.anm", 0xc2f84caaf0fa173c },
		{ "stgenm_al.anm", 0x31c2a38a5e72dca6 },
		{ "stgenm_rm.anm", 0x3739b47e98df8217 },
		{ "stgenm_sk.anm", 0x127defba2d418e1b },
		{ "stgenm_yk.anm", 0xa8c13c91325acbcd },
		{ "stgenm_ym.anm", 0xd2386b7b7ab7f2c },
		{ "stgenm_yy.anm", 0xc07bcc10e1c5c0ba },
		{ "text.anm", 0x6be44d63227ff82c },
		{ "th08_00.mid", 0x84e5ea3223e82715 },
		{ "th08_01.mid", 0x4cbeec0139e61a21 },
		{ "th08_0100d.ver", 0xe090cea154e0cbd2 },
		{ "th08_03.mid", 0x8c766a2df9d601af },
		{ "th08_04.mid", 0x5481e4a062907a41 },
		{ "th08_05.mid", 0xfa58cd72d1d1335 },
		{ "th08_06.mid", 0xc66d3cddc04b90ca },
		{ "th08_07.mid", 0x1aef9ada7aca779 },
		{ "th08_08.mid", 0xa91362e24da11879 },
		{ "th08_09.mid", 0xf98e7de7a7d83972 },
		{ "th08_10.mid", 0xbb0d9e52c56fc7b0 },
		{ "th08_11.mid", 0xb2c0fb2f09c4c351 },
		{ "th08_12.mid", 0x6d62e3d98ffc3bdf },
		{ "th08_13.mid", 0xe2d173d8e985c80f },
		{ "th08_13b.mid", 0x98bc84b1be535a0f },
		{ "th08_14.mid", 0x9622b34fc1f096ed },
		{ "th08_15.mid", 0x2b2081efe41fef14 },
		{ "th08_16.mid", 0x5f5eba2bc4bc15cc },
		{ "th08_17.mid", 0xaba9b83761acec9a },
		{ "th08_18.mid", 0x1a5e98a9ec0e0bfd },
		{ "th08_19.mid", 0x79dd9ea5fdcc2778 },
		{ "th08_20.mid", 0xe01abc91a5db4eb2 },
		{ "th08logo.jpg", 0x45773b936892204a },
		{ "thbgm.fmt", 0x3d76b644f537bf51 },
		{ "times.anm", 0x327a718dba907fd3 },
		{ "title00.png", 0x1afbc16b3c5b33c8 },
		{ "title01.anm", 0x8074a87c6f5e81a }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th08";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th08-test.dat";

	public ArchiveTh08Tests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\th08.dat")]
	public void ReadArchiveTh08(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.IN, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH08.PBGZ>(archive);
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
	[InlineData($"{TEST_PATH}\\th08.dat", true)]
	public async Task ReadArchiveTh08Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.IN, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsType<TH08.PBGZ>(archive);
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
	public void WriteArchiveTh08(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.IN, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.IN, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh08Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.IN, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.IN, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
