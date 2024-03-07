using System.Collections.Frozen;
using System.IO.Hashing;
using System.Text;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh145Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "data/actor/ObjectCommon.nut", 0x116180c7d9cd626b },
		{ "data/actor/buffScript.nut", 0xd9ff1cddc183a9cb },
		{ "data/actor/common/common.pat", 0x5c45584e694fe643 },
		{ "data/actor/common/occultF_back0001.png", 0xe7561156d8471cf },
		{ "data/actor/common/occultF_back0002.png", 0xbbb2a0b4720ab014 },
		{ "data/actor/common/occult_B_Wall0000.png", 0x9df68c60147a8a40 },
		{ "data/actor/common/texture/occult_trail0000.png", 0xa5b84c03c7d9e89f },
		{ "data/actor/cpuCommon.nut", 0x34af4bcf650a53cd },
		{ "data/actor/effect.nut", 0xaae0e84569515408 },
		{ "data/actor/effect/effect.pat", 0x196ee26ca050820c },
		{ "data/actor/effect/texture/chargeParticle0000.dds", 0x4538cc8bbe059658 },
		{ "data/actor/effect/texture/moji_doubleKo.png", 0x68824a55d81e39fc },
		{ "data/actor/effect/texture/moji_dowbleKO_jp.png", 0x2328165382878ba9 },
		{ "data/actor/effect/texture/moji_draw.png", 0x19b284c13b333263 },
		{ "data/actor/effect/texture/moji_draw_jp.png", 0x51d8662a8b368447 },
		{ "data/actor/effect/texture/moji_fight.png", 0x3b85d98c8b34e4ab },
		{ "data/actor/effect/texture/moji_fight_jp.png", 0xdc8131a2b8add5c2 },
		{ "data/actor/effect/texture/moji_ko.png", 0xd8e0ebe1a0a0b006 },
		{ "data/actor/effect/texture/moji_ko_jp.png", 0xcc6defa498a5fd2f },
		{ "data/actor/effect/texture/moji_ready.png", 0x47ff634b8928aadd },
		{ "data/actor/effect/texture/moji_ready_jp.png", 0xa31dd7b73a6c492a },
		{ "data/actor/effect/texture/moji_timeUp_jp.png", 0x561b9049e21b7fe7 },
		{ "data/actor/effect/texture/moji_timeup.png", 0xe5b6e4768370b80a },
		{ "data/actor/effect/texture/moji_winner_1p.png", 0xcf0b672ea012bd2b },
		{ "data/actor/effect/texture/moji_winner_2p.png", 0x424b32ccafbaa4a3 },
		{ "data/actor/effect/texture/moji_winner_jp.png", 0x1742e18d77327f10 },
		{ "data/actor/effect/texture/namelist_left.png", 0x2e48bb160723d654 },
		{ "data/actor/effect/texture/namelist_right.png", 0xfefdf057854abc8d },
		{ "data/actor/effect/texture/occultAura0000.dds", 0x751f4346945c2342 },
		{ "data/actor/effect/texture/spellRing0000.png", 0x99851d1b664fe67a },
		{ "data/actor/effect/texture/spellRing0001.png", 0x5b7536d5eeae9b57 },
		{ "data/actor/effect/texture/spot_name0000.png", 0x4c0a696bd3e007f1 },
		{ "data/actor/effect/texture/testFront0000.dds", 0x3811b808e2b1dac6 },
		{ "data/actor/futo.nut", 0x6392fbf97fa37efe },
		{ "data/actor/futo/futo.pat", 0x2f319dfc12b29114 },
		{ "data/actor/futo/occultA0000.bmp", 0x7cb93fd0537c520f },
		{ "data/actor/futo/occultA0001.bmp", 0xb6a0df3b8867013e },
		{ "data/actor/futo/occultA0002.bmp", 0xaa769d2bda97012f },
		{ "data/actor/futo/occultA0003.bmp", 0x4eb85f51e6b6a196 },
		{ "data/actor/futo/occultA0004.bmp", 0xa8220d8c11ad5b1f },
		{ "data/actor/futo/occultB0000.bmp", 0xf99cd0e6060b7179 },
		{ "data/actor/futo/occultB0001.bmp", 0x98a30246470f1315 },
		{ "data/actor/futo/occultB0002.bmp", 0xb1ea35f9e0f183ad },
		{ "data/actor/futo/occultB0003.bmp", 0x5891dcda9a7e93c1 },
		{ "data/actor/futo/occultB0004.bmp", 0x825e567f9767f49c },
		{ "data/actor/futo/occultB0005.bmp", 0x977afd1180df60fb },
		{ "data/actor/futo/occultB0006.bmp", 0xf5298b9edf91be2b },
		{ "data/actor/futo/occultC0000.bmp", 0xa2c948018bfbf651 },
		{ "data/actor/futo/occultC0001.bmp", 0x691a85e16770ab87 },
		{ "data/actor/futo/occultC0002.bmp", 0x44d518994a96a70 },
		{ "data/actor/futo/occultC0003.bmp", 0x90e57585df9d6d83 },
		{ "data/actor/futo/occultC0004.bmp", 0x765fac4043d9c82d },
		{ "data/actor/futo/occultC0005.bmp", 0x824cc95d8d09d108 },
		{ "data/actor/futo/palette000.bmp", 0xe9b56e2c8b0ff33d },
		{ "data/actor/futo/palette001.bmp", 0x7f66a49f1cf358dd },
		{ "data/actor/futo/palette002.bmp", 0x6c4917ba179d6ceb },
		{ "data/actor/futo/palette003.bmp", 0x7508fe94f23bdcf1 },
		{ "data/actor/futo/palette004.bmp", 0x6124e5f7a94de09c },
		{ "data/actor/futo/palette005.bmp", 0x95c81cd512df7357 },
		{ "data/actor/futo/palette006.bmp", 0x2833bc009b7406a6 },
		{ "data/actor/futo/palette007.bmp", 0xd6c69ed027d38d77 },
		{ "data/actor/futo/texture/dishA0000.png", 0x94f93888e301fcd0 },
		{ "data/actor/futoInit.nut", 0xd8ec162e9e9b9669 },
		{ "data/actor/futo_base.nut", 0x8fefb808c9d8000 },
		{ "data/actor/futo_boss.nut", 0xdfd52d860c3f4600 },
		{ "data/actor/futo_cpu.nut", 0xc22e43dcb19088b3 },
		{ "data/actor/futo_shot.nut", 0xb78335a4aafcc7ba },
		{ "data/actor/hanako.nut", 0x95de6d107f4ef358 },
		{ "data/actor/hanako/hanako.pat", 0xad727f09c09996c5 },
		{ "data/actor/hanako/texture/occult_spark.png", 0x9369ad43eb9f6abc },
		{ "data/actor/hanako/texture/skillF_Object0001.dds", 0xde37e4ef20d181c6 },
		{ "data/actor/hanako/winA0000.bmp", 0xd56de500f312abee },
		{ "data/actor/hanako/winA0001.bmp", 0xcd5c3ce2979d8d1d },
		{ "data/actor/hanako/winA0002.bmp", 0xcecd54055708f0ab },
		{ "data/actor/hanakoInit.nut", 0xc9c3b62094b43d3e },
		{ "data/actor/hanako_cpu.nut", 0x66af92c62074142f },
		{ "data/actor/hijiri.nut", 0x7020b9c24386bf9e },
		{ "data/actor/hijiri/hijiri.pat", 0xdbcb8815f0c3384c },
		{ "data/actor/hijiri/palette000.bmp", 0xd43f969546453b3 },
		{ "data/actor/hijiri/palette001.bmp", 0xe066b699c51d1001 },
		{ "data/actor/hijiri/palette002.bmp", 0x642fb15be4e0643b },
		{ "data/actor/hijiri/palette003.bmp", 0xdcd4fbb8c0a07e86 },
		{ "data/actor/hijiri/palette004.bmp", 0xba83a47d4b51121c },
		{ "data/actor/hijiri/palette005.bmp", 0x663d70800f60b73e },
		{ "data/actor/hijiri/palette006.bmp", 0xbe40bb468523676a },
		{ "data/actor/hijiri/palette007.bmp", 0x2e0198ac0bb9e0e3 },
		{ "data/actor/hijiri/texture/bikeLight0000.png", 0xd0a2b72efd5de95b },
		{ "data/actor/hijiri/texture/bike_back0000.bmp", 0x27d9ab67185be3a8 },
		{ "data/actor/hijiri/texture/scrollA0000.bmp", 0x60eedb2f4de97f40 },
		{ "data/actor/hijiri/texture/scrollA0001.bmp", 0x2129bb4e8af0cba4 },
		{ "data/actor/hijiri/texture/scrollA0002.bmp", 0xbc79e3322787608a },
		{ "data/actor/hijiri/texture/scrollA0003.bmp", 0x94415d0503f4b2aa },
		{ "data/actor/hijiri/texture/scrollA0004.bmp", 0x7a770e176443262a },
		{ "data/actor/hijiri/texture/scrollA0005.bmp", 0xa5b9c42196a9a665 },
		{ "data/actor/hijiri/texture/scrollC0000.bmp", 0x5371cda09f80660d },
		{ "data/actor/hijiri/texture/scrollC0001.bmp", 0xae716530a845c938 },
		{ "data/actor/hijiri/texture/scrollC0002.bmp", 0xbc459ed47e60af7c },
		{ "data/actor/hijiri/texture/scrollC0003.bmp", 0xc386f714cd0630d8 },
		{ "data/actor/hijiri/texture/scrollC0004.bmp", 0x5b2ed5f8d895c950 },
		{ "data/actor/hijiri/texture/scrollC0005.bmp", 0x9793aa2f5f22a3a4 },
		{ "data/actor/hijiriInit.nut", 0x7101b3620c8c8fee },
		{ "data/actor/hijiri_base.nut", 0x9e2ec6540a1140ab },
		{ "data/actor/hijiri_boss.nut", 0x21c229241bf89b8e },
		{ "data/actor/hijiri_cpu.nut", 0xa57c6719eaceb137 },
		{ "data/actor/hijiri_cpuBoss.nut", 0xf6d6a641615e7efe },
		{ "data/actor/hijiri_r.nut", 0x82524d3441801f69 },
		{ "data/actor/hijiri_shot.nut", 0xeb6af2409bfc4eb5 },
		{ "data/actor/ichirin.nut", 0x852c54d147ee0f60 },
		{ "data/actor/ichirin/ichirin.pat", 0x20272210370307aa },
		{ "data/actor/ichirin/occultObject0000.bmp", 0x1cc9696ba4a40a42 },
		{ "data/actor/ichirin/occultObject0001.bmp", 0xba51df485789fd5a },
		{ "data/actor/ichirin/occultObject0002.bmp", 0xe95c3d6a6d3f0269 },
		{ "data/actor/ichirin/occultObject0003.bmp", 0xc55afd4de84c2560 },
		{ "data/actor/ichirin/occultObject0004.bmp", 0x1642d2f825840c51 },
		{ "data/actor/ichirin/occultObject0005.bmp", 0x1a12f5c0a3d2cf5 },
		{ "data/actor/ichirin/palette000.bmp", 0x9505f1acb735e28c },
		{ "data/actor/ichirin/palette001.bmp", 0x71d31c8ec87b9e2c },
		{ "data/actor/ichirin/palette002.bmp", 0x755b10b664d0ab32 },
		{ "data/actor/ichirin/palette003.bmp", 0xb4bc651467675c9 },
		{ "data/actor/ichirin/palette004.bmp", 0x2158a553d63c2c2e },
		{ "data/actor/ichirin/palette005.bmp", 0xe3e83bc21114dd43 },
		{ "data/actor/ichirin/palette006.bmp", 0xf80161d109bc86e0 },
		{ "data/actor/ichirin/palette007.bmp", 0xdd1f0156771aca37 },
		{ "data/actor/ichirinInit.nut", 0x1d3be066b13a36e5 },
		{ "data/actor/ichirin_base.nut", 0xe11428ea0af8bc43 },
		{ "data/actor/ichirin_boss.nut", 0x672c9bdc7e6184c2 },
		{ "data/actor/ichirin_cpu.nut", 0xf8f217d7ba777c3e },
		{ "data/actor/ichirin_cpuBoss.nut", 0x12bb4c162d5ec257 },
		{ "data/actor/ichirin_shot.nut", 0xefcad25e6f432cbc },
		{ "data/actor/kasen.nut", 0x87719378cb45d9a4 },
		{ "data/actor/kasen/climax_CutFace.png", 0xf57058a7f6bec480 },
		{ "data/actor/kasen/kasen.pat", 0x635d6c7c281d7693 },
		{ "data/actor/kasen/palette000.bmp", 0xf82957f341d989d3 },
		{ "data/actor/kasenInit.nut", 0x418cafda5d588a99 },
		{ "data/actor/kasen_base.nut", 0xc1b250364a6a928d },
		{ "data/actor/kasen_boss.nut", 0x5947c7cbac1824c9 },
		{ "data/actor/kasen_cpu.nut", 0x49cbc313450f3fc6 },
		{ "data/actor/kasen_eagle.nut", 0xf16809606101ab1 },
		{ "data/actor/kasen_shot.nut", 0xc5fe514a7376d26e },
		{ "data/actor/koishi.nut", 0x64c8464487032728 },
		{ "data/actor/koishi/koishi.pat", 0xde59b223f65db8bf },
		{ "data/actor/koishi/palette000.bmp", 0xb60038258eff9d0c },
		{ "data/actor/koishi/palette001.bmp", 0x69ba34d9a6957d93 },
		{ "data/actor/koishi/palette002.bmp", 0x88af1707f515e00c },
		{ "data/actor/koishi/palette003.bmp", 0x3afa320fef43960d },
		{ "data/actor/koishi/palette004.bmp", 0x2ff88207ef3334a2 },
		{ "data/actor/koishi/palette005.bmp", 0x133b036607e32f5d },
		{ "data/actor/koishi/palette006.bmp", 0x539ac5ec63f851bc },
		{ "data/actor/koishi/palette007.bmp", 0xbf15799c8f7a0b87 },
		{ "data/actor/koishi/texture/ball2.bmp", 0x46dc30a717352260 },
		{ "data/actor/koishi/texture/ball3.png", 0xdffbeee18280c6c5 },
		{ "data/actor/koishiInit.nut", 0xbbf07b9f87636658 },
		{ "data/actor/koishi_base.nut", 0x86637816cbadff66 },
		{ "data/actor/koishi_boss.nut", 0x6808ecbbdb53d4a1 },
		{ "data/actor/koishi_cpu.nut", 0x902041c64f26a32d },
		{ "data/actor/koishi_shot.nut", 0xce49585da34adb64 },
		{ "data/actor/kokoro.nut", 0x5798f97449c42a46 },
		{ "data/actor/kokoro/kokoro.pat", 0x854b380439f4dba3 },
		{ "data/actor/kokoro/kokoro_ed.pat", 0xe7af967d415349bd },
		{ "data/actor/kokoro/palette000.bmp", 0x9981aa3e28e21041 },
		{ "data/actor/kokoro/palette001.bmp", 0xec0d77165bd3aa05 },
		{ "data/actor/kokoro/palette002.bmp", 0x3d9f9b0f732c0170 },
		{ "data/actor/kokoro/palette003.bmp", 0x1b6edb3385fa9732 },
		{ "data/actor/kokoro/palette004.bmp", 0xefae5fcf781d2edc },
		{ "data/actor/kokoro/palette005.bmp", 0x171402f1553d08e2 },
		{ "data/actor/kokoro/palette006.bmp", 0x5f277de655c88ec6 },
		{ "data/actor/kokoro/palette007.bmp", 0x627ceddbb5472e95 },
		{ "data/actor/kokoro/texture/cut0000.png", 0x7604bda811cd612a },
		{ "data/actor/kokoro/texture/cut0001.png", 0x322d03e15d1722ae },
		{ "data/actor/kokoro/texture/cut0002.png", 0xfabfd3aa6d762116 },
		{ "data/actor/kokoro/texture/cut0003.png", 0x1fab8bd55bc18560 },
		{ "data/actor/kokoroInit.nut", 0x504c8c5a439b3f51 },
		{ "data/actor/kokoro_base.nut", 0x4f5325a12f1b7d2 },
		{ "data/actor/kokoro_boss.nut", 0xc24015f79cd7a877 },
		{ "data/actor/kokoro_cpu.nut", 0x99cb0f0a65013f1f },
		{ "data/actor/kokoro_shot.nut", 0xbdb5846dff216001 },
		{ "data/actor/mamizou.nut", 0xa7bb39a1f697ce20 },
		{ "data/actor/mamizou/mamizou.pat", 0xe68b4f72757d58e6 },
		{ "data/actor/mamizou/palette000.bmp", 0x7201dec6f85c25b4 },
		{ "data/actor/mamizou/palette001.bmp", 0x6f23a25c67654b34 },
		{ "data/actor/mamizou/palette002.bmp", 0xe6b25268cf46d8ee },
		{ "data/actor/mamizou/palette003.bmp", 0x95138e207a7472a2 },
		{ "data/actor/mamizou/palette004.bmp", 0xe2399b926c9d622c },
		{ "data/actor/mamizou/palette005.bmp", 0x21f60d21bd431ed9 },
		{ "data/actor/mamizou/palette006.bmp", 0x8cd4dc13234ad3cd },
		{ "data/actor/mamizou/palette007.bmp", 0x3502e47319b93aa6 },
		{ "data/actor/mamizou/texture/occult_cap0001.dds", 0x33e43e14dc536b7e },
		{ "data/actor/mamizou/texture/occult_cap0002.dds", 0xe26658ce35ae77f5 },
		{ "data/actor/mamizouInit.nut", 0x58e7f6ec26dfb0f7 },
		{ "data/actor/mamizou_base.nut", 0xe37f2c88bd2196b7 },
		{ "data/actor/mamizou_boss.nut", 0x8f1344e309b6681c },
		{ "data/actor/mamizou_cpu.nut", 0x6c3e54f8d3f7de10 },
		{ "data/actor/mamizou_cpuBoss.nut", 0x45ba1913ce229653 },
		{ "data/actor/mamizou_shot.nut", 0x9270c8db61f8645f },
		{ "data/actor/marisa.nut", 0xe54c0ee7f2227151 },
		{ "data/actor/marisa/climax_BackBase.png", 0x59cf618a24e8f74c },
		{ "data/actor/marisa/marisa.pat", 0x80db8472f9cab177 },
		{ "data/actor/marisa/palette000.bmp", 0xc95835283764b134 },
		{ "data/actor/marisa/palette001.bmp", 0x95edefb72d8e595 },
		{ "data/actor/marisa/palette002.bmp", 0xaca3eb7c7f2ec196 },
		{ "data/actor/marisa/palette003.bmp", 0x593c700170238bbc },
		{ "data/actor/marisa/palette004.bmp", 0x2d8e147c299e191d },
		{ "data/actor/marisa/palette005.bmp", 0x339ff2786c1763b4 },
		{ "data/actor/marisa/palette006.bmp", 0xa629caabead06753 },
		{ "data/actor/marisa/palette007.bmp", 0x7871435646132e89 },
		{ "data/actor/marisa/texture/ball2.bmp", 0x46dc30a717352260 },
		{ "data/actor/marisa/texture/ball3.png", 0xdffbeee18280c6c5 },
		{ "data/actor/marisaInit.nut", 0x5df087f3b2e2eae7 },
		{ "data/actor/marisa_base.nut", 0x3c088fee5746b283 },
		{ "data/actor/marisa_boss.nut", 0xf06d970bc0ca25e6 },
		{ "data/actor/marisa_cpu.nut", 0xea5fd327eddac026 },
		{ "data/actor/marisa_cpuBoss.nut", 0xd4c55cadb1d26dd0 },
		{ "data/actor/marisa_shot.nut", 0x549ca2aa86a1ca38 },
		{ "data/actor/miko.nut", 0xc1c3708b442ae9c0 },
		{ "data/actor/miko/miko.pat", 0x504f53cfbd0d3794 },
		{ "data/actor/miko/palette000.bmp", 0xc4af271d48eadf65 },
		{ "data/actor/miko/palette001.bmp", 0x69b9aae10c608c06 },
		{ "data/actor/miko/palette002.bmp", 0xdef6723fe436198 },
		{ "data/actor/miko/palette003.bmp", 0x4f9b15c5e7699b12 },
		{ "data/actor/miko/palette004.bmp", 0x16db267cf8e7a03a },
		{ "data/actor/miko/palette005.bmp", 0xd7450c7d2971cdda },
		{ "data/actor/miko/palette006.bmp", 0xa0ee8f2f0b9b32b6 },
		{ "data/actor/miko/palette007.bmp", 0x420672bdbfb6eb85 },
		{ "data/actor/miko/texture/box_texture0000.bmp", 0x27d9ab67185be3a8 },
		{ "data/actor/miko/texture/occult_select0000.dds", 0x5febacfcd3f754b5 },
		{ "data/actor/miko/texture/occult_select0001.dds", 0xac559eb8dd8b7c67 },
		{ "data/actor/mikoInit.nut", 0x74ab62430641627f },
		{ "data/actor/miko_base.nut", 0xc19c4ab361aaf1a8 },
		{ "data/actor/miko_boss.nut", 0xa464a6db3a9735f6 },
		{ "data/actor/miko_cpu.nut", 0xcb60b19077775740 },
		{ "data/actor/miko_r.nut", 0xc01ae8812b376116 },
		{ "data/actor/miko_shot.nut", 0xf1229fc2b5841df4 },
		{ "data/actor/mokou.nut", 0xe56e6d9455149577 },
		{ "data/actor/mokou/mokou.pat", 0x2de8bcebe6ff8fc7 },
		{ "data/actor/mokou/palette000.bmp", 0x61bf2229d4280d23 },
		{ "data/actor/mokou/palette001.bmp", 0xaefd09cfbab611cc },
		{ "data/actor/mokou/palette002.bmp", 0xed51a9f6204fdedd },
		{ "data/actor/mokou/palette003.bmp", 0xb7a5b4148c856ca7 },
		{ "data/actor/mokou/palette004.bmp", 0x6fd5472e185ef1af },
		{ "data/actor/mokou/palette005.bmp", 0x72246808043ba655 },
		{ "data/actor/mokou/palette006.bmp", 0x2d70376a0f72cce },
		{ "data/actor/mokou/palette007.bmp", 0x30b55dfe18edd605 },
		{ "data/actor/mokou/texture/skillF_mokoFireB0000.png", 0xb16776dea5e4c084 },
		{ "data/actor/mokou/texture/skillF_mokoFireB0001.png", 0xcd3c702abec8036c },
		{ "data/actor/mokou/texture/skillF_mokoFireB0002.png", 0x2f16a141276670ba },
		{ "data/actor/mokou/texture/skillF_mokoFireB0003.png", 0x63b90be2272f3222 },
		{ "data/actor/mokou/texture/skillF_mokoFireB0004.png", 0x69985e059ad04b4e },
		{ "data/actor/mokou/texture/skillF_mokoFireB0005.png", 0x2451f8b94f420491 },
		{ "data/actor/mokou/texture/spellA_fire0001.png", 0xbf7e5962cba234b6 },
		{ "data/actor/mokou/texture/spellA_fireVortex0003.png", 0xff8c6edc047e03e6 },
		{ "data/actor/mokou/texture/spellA_fireVortex0004.png", 0xa935ebe0cd0e52de },
		{ "data/actor/mokou/texture/spellA_fireVortex0005.png", 0x674e9524a80e0263 },
		{ "data/actor/mokou/wingF0001.bmp", 0xe697f00e6a52db46 },
		{ "data/actor/mokou/wingF0002.bmp", 0xdfcebbc54feabfb4 },
		{ "data/actor/mokou/wingF0003.bmp", 0x8ce92772c04d9371 },
		{ "data/actor/mokou/wingF0004.bmp", 0x8fa21bf241f3b4a7 },
		{ "data/actor/mokou/wingF0005.bmp", 0x357e1f4664ef9d6d },
		{ "data/actor/mokou/wingF0006.bmp", 0xa0ef1735d11ab6a3 },
		{ "data/actor/mokou/wingF0007.bmp", 0xf80757a1fe339206 },
		{ "data/actor/mokouInit.nut", 0xb3b882bb283c5446 },
		{ "data/actor/mokou_base.nut", 0xcd339dc854b5011 },
		{ "data/actor/mokou_boss.nut", 0xb2591ffa82b886de },
		{ "data/actor/mokou_cpu.nut", 0x8f0b333271431e86 },
		{ "data/actor/mokou_shot.nut", 0x8bbcaec39dea5475 },
		{ "data/actor/neutralInit.nut", 0x9ce535c95838bf13 },
		{ "data/actor/nitori.nut", 0xe1ab53ffdb10439f },
		{ "data/actor/nitori/nitori.pat", 0xd0a84aac87017ac5 },
		{ "data/actor/nitori/palette000.bmp", 0x85b22d69a4f4aea },
		{ "data/actor/nitori/palette001.bmp", 0xc278eb7408665526 },
		{ "data/actor/nitori/palette002.bmp", 0xfbe24b685d7c4815 },
		{ "data/actor/nitori/palette003.bmp", 0x1b4f43054abc06bb },
		{ "data/actor/nitori/palette004.bmp", 0x50a0e6eb4ec93871 },
		{ "data/actor/nitori/palette005.bmp", 0x6c4a180bb3a5ecc1 },
		{ "data/actor/nitori/palette006.bmp", 0xffe3865352c20f69 },
		{ "data/actor/nitori/palette007.bmp", 0xcf4b452d92d45116 },
		{ "data/actor/nitoriInit.nut", 0x90a429eee35090e7 },
		{ "data/actor/nitori_base.nut", 0x8bd0f0f090ef69c3 },
		{ "data/actor/nitori_boss.nut", 0xa2fcdd700cc08e6b },
		{ "data/actor/nitori_cpu.nut", 0xd89df7ba51fea53c },
		{ "data/actor/nitori_shot.nut", 0xb608bf6b6e4daee8 },
		{ "data/actor/playerCommonScript.nut", 0xd6ec9372761ceb30 },
		{ "data/actor/reimu.nut", 0xabe621a4181031bd },
		{ "data/actor/reimu/palette000.bmp", 0xe85d76d3ed68c9f0 },
		{ "data/actor/reimu/palette001.bmp", 0x46553efafa520d03 },
		{ "data/actor/reimu/palette002.bmp", 0x57594732b54124a7 },
		{ "data/actor/reimu/palette003.bmp", 0xd9a250f5361e362e },
		{ "data/actor/reimu/palette004.bmp", 0x1fa9509ed417f5e0 },
		{ "data/actor/reimu/palette005.bmp", 0x25690395c06b619e },
		{ "data/actor/reimu/palette006.bmp", 0xcd5925ea85380aba },
		{ "data/actor/reimu/palette007.bmp", 0x5f164150432db02b },
		{ "data/actor/reimu/reimu.pat", 0xbfb4f9cfed84f6bf },
		{ "data/actor/reimuInit.nut", 0xfccc1e95f49c979d },
		{ "data/actor/reimu_base.nut", 0x3d0af51bbef9d54c },
		{ "data/actor/reimu_boss.nut", 0xbe93943a5110bad8 },
		{ "data/actor/reimu_cpu.nut", 0x119be0160c180503 },
		{ "data/actor/reimu_shot.nut", 0x602b60e125a5fc4e },
		{ "data/actor/sinmyoumaru.nut", 0x61eea579633f48b0 },
		{ "data/actor/sinmyoumaru/palette000.bmp", 0x41c62dbe5b48e224 },
		{ "data/actor/sinmyoumaru/palette001.bmp", 0x6781033284440b4e },
		{ "data/actor/sinmyoumaru/palette002.bmp", 0x89fbdcb6ea2bba81 },
		{ "data/actor/sinmyoumaru/palette003.bmp", 0x4ae89ff7ef188958 },
		{ "data/actor/sinmyoumaru/palette004.bmp", 0x82c735075a1d3e1c },
		{ "data/actor/sinmyoumaru/palette005.bmp", 0xffcea8f0bfa86fde },
		{ "data/actor/sinmyoumaru/palette006.bmp", 0xa8da92aaefaafa0c },
		{ "data/actor/sinmyoumaru/palette007.bmp", 0x1678338d466a4589 },
		{ "data/actor/sinmyoumaru/sinmyoumaru.pat", 0xae7c201a1fbbc5bc },
		{ "data/actor/sinmyoumaru/texture/climax_giant0003.png", 0xc67682bdce0c9c1b },
		{ "data/actor/sinmyoumaruInit.nut", 0x9c3af84828faf1f8 },
		{ "data/actor/sinmyoumaru_base.nut", 0x3a7e3a8539163906 },
		{ "data/actor/sinmyoumaru_boss.nut", 0xce20c6ef6c58521c },
		{ "data/actor/sinmyoumaru_cpu.nut", 0xe3e833ce81b6d8ed },
		{ "data/actor/sinmyoumaru_shot.nut", 0x8b4d7613b43e967b },
		{ "data/actor/story/futo/story.pat", 0xd91b0851ebfee9e3 },
		{ "data/actor/story/futo/texture/story_moji_title.png", 0x5bcf751e4942ebe4 },
		{ "data/actor/story/hijiri/story.pat", 0x63a4a5f9c79d62dd },
		{ "data/actor/story/hijiri/texture/story_moji_title.png", 0xd9849a6afa706f42 },
		{ "data/actor/story/ichirin/story.pat", 0x667df594a6638251 },
		{ "data/actor/story/ichirin/texture/story_moji_title.png", 0x6f8c2bca293c076 },
		{ "data/actor/story/kasen/story.pat", 0x920a3ab80ae52c70 },
		{ "data/actor/story/kasen/texture/story_moji_title.png", 0xddfded3c480320c7 },
		{ "data/actor/story/koishi/story.pat", 0x667df594a6638251 },
		{ "data/actor/story/koishi/texture/story_moji_title.png", 0xdc3916c2d57db499 },
		{ "data/actor/story/kokoro/story.pat", 0x667df594a6638251 },
		{ "data/actor/story/kokoro/texture/story_moji_title.png", 0xfa041dc6ea28ccca },
		{ "data/actor/story/mamizou/story.pat", 0x920a3ab80ae52c70 },
		{ "data/actor/story/mamizou/texture/story_moji_title.png", 0x5b5f4fff6654a5ea },
		{ "data/actor/story/marisa/story.pat", 0x920a3ab80ae52c70 },
		{ "data/actor/story/marisa/texture/story_moji_title.png", 0x24a8111b900c3f2 },
		{ "data/actor/story/miko/story.pat", 0x920a3ab80ae52c70 },
		{ "data/actor/story/miko/texture/story_moji_title.png", 0x8fa45c96b874e0fc },
		{ "data/actor/story/mokou/story.pat", 0x920a3ab80ae52c70 },
		{ "data/actor/story/mokou/texture/story_moji_title.png", 0x2eb3bb7b02aa62d5 },
		{ "data/actor/story/nitori/story.pat", 0x667df594a6638251 },
		{ "data/actor/story/nitori/texture/story_moji_title.png", 0x4939b46172fffb46 },
		{ "data/actor/story/reimu/story.pat", 0x6ffc0133261fcfec },
		{ "data/actor/story/reimu/texture/story_moji_title.png", 0x771959ea2615d13b },
		{ "data/actor/story/reimu/tips0002b.png", 0xa0142b8407ccb205 },
		{ "data/actor/story/reimu/tips0003b.png", 0x33eb46451211f049 },
		{ "data/actor/story/reimuB/story.pat", 0x667df594a6638251 },
		{ "data/actor/story/reimuB/texture/story_moji_title.png", 0x1e9f31c90e95bc73 },
		{ "data/actor/story/sinmyoumaru/story.pat", 0x9bba55486529c7d6 },
		{ "data/actor/story/sinmyoumaru/texture/story_moji_title.png", 0x3324fa8bb6bf1ad1 },
		{ "data/actor/story/usami/story.pat", 0x440a2a5cce68d15 },
		{ "data/actor/story/usami/texture/story_moji_title.png", 0x9de23dabd71f6ce },
		{ "data/actor/storyEffect.nut", 0x74227c9dad649e9c },
		{ "data/actor/usami.nut", 0x2d7ac137aedd048d },
		{ "data/actor/usami/mask_skt0000.bmp", 0xc17ade8a82c6a3f3 },
		{ "data/actor/usami/palette000.bmp", 0xe0e9145e37626ead },
		{ "data/actor/usami/palette001.bmp", 0x5bef5fb3d2945861 },
		{ "data/actor/usami/palette002.bmp", 0xf17d5f17f7d3094e },
		{ "data/actor/usami/palette003.bmp", 0x18d175971de54e1c },
		{ "data/actor/usami/palette004.bmp", 0x1a0df2a060b4bb38 },
		{ "data/actor/usami/palette005.bmp", 0x683d2ea57447ea0 },
		{ "data/actor/usami/palette006.bmp", 0x7fb96116943be9d6 },
		{ "data/actor/usami/palette007.bmp", 0x46f7c76bfd378fe0 },
		{ "data/actor/usami/texture/mantle000.png", 0x19c6499c4d9cad63 },
		{ "data/actor/usami/usami.pat", 0xdca76d94d0fb92cf },
		{ "data/actor/usamiInit.nut", 0x1878570b85d3201e },
		{ "data/actor/usami_base.nut", 0x16b1dcc7303e4d08 },
		{ "data/actor/usami_boss.nut", 0x693b3d8e7cc02da6 },
		{ "data/actor/usami_cpu.nut", 0xf2f59d04267580c },
		{ "data/actor/usami_cpuBoss.nut", 0x581fe3036312a24d },
		{ "data/actor/usami_shot.nut", 0xbcf877e471f7451b },
		{ "data/csv/spellCard/futo.csv", 0x8b8a5ea6d9e928b6 },
		{ "data/csv/spellCard/ichirin.csv", 0x15c2f7c79ce5e003 },
		{ "data/csv/spellCard/kokoro.csv", 0xd2a61d0ef06f6ac8 },
		{ "data/csv/spellCard/marisa.csv", 0xa6c36817850daae4 },
		{ "data/csv/spellCard/sinmyoumaru.csv", 0x9b70112d13b6bff8 },
		{ "data/effect/Texture/Effect/FlashB.nhtex", 0x4bae40b130621d75 },
		{ "data/effect/Texture/Effect/Loop_streem0000.nhtex", 0x7a182c70954a1803 },
		{ "data/effect/Texture/Effect/MaskTest.nhtex", 0x3371017e86d712f6 },
		{ "data/effect/Texture/Effect/RingFlow.nhtex", 0x2eb88a9a5e71b26e },
		{ "data/effect/Texture/Effect/Ring_violet0000.nhtex", 0x6e135cc6e9dd8818 },
		{ "data/effect/Texture/Effect/SphereRY.nhtex", 0x12707faecb22f840 },
		{ "data/effect/Texture/Effect/SphereW.nhtex", 0x8526b3fe99616249 },
		{ "data/effect/Texture/Effect/Wind.nhtex", 0x1cbcdd19598b6a5e },
		{ "data/effect/Texture/Effect/baria0000.nhtex", 0xe21f0e9d80e7227a },
		{ "data/effect/Texture/Effect/bariaMask0000.nhtex", 0xed469b14783bc396 },
		{ "data/effect/Texture/Effect/chargeLineMap.nhtex", 0x9e1ccc20b76da096 },
		{ "data/effect/Texture/Effect/chargeParticle0000.nhtex", 0x952a049f7d75ec0d },
		{ "data/effect/Texture/Effect/commonFlash0000.nhtex", 0xf1cbd3cc0d772a37 },
		{ "data/effect/Texture/Effect/commonFlashRing_mono0000.nhtex", 0x905998c5d6e0cdf2 },
		{ "data/effect/Texture/Effect/commonFlash_blue0000.nhtex", 0xb8310b009eef9fd4 },
		{ "data/effect/Texture/Effect/commonFlash_eme0000.nhtex", 0xc4b5723e519b5d00 },
		{ "data/effect/Texture/Effect/commonFlash_yellow0000.nhtex", 0xd4a48a867eeb884e },
		{ "data/effect/Texture/Effect/common_hitSpark0000.nhtex", 0xe9cc2fa5b99adc54 },
		{ "data/effect/Texture/Effect/dashLine0000.nhtex", 0x2168566fa27a8371 },
		{ "data/effect/Texture/Effect/dash_wave0000.nhtex", 0x4f5256c28152969a },
		{ "data/effect/Texture/Effect/fadeBall0000.nhtex", 0x45aa1039d8d21b10 },
		{ "data/effect/Texture/Effect/fall_fire0000.nhtex", 0xe9bb2461c51e2b48 },
		{ "data/effect/Texture/Effect/fire0000.nhtex", 0x7fd9db688e978a28 },
		{ "data/effect/Texture/Effect/fireChip0000.nhtex", 0xfa06dbfe83b9bc03 },
		{ "data/effect/Texture/Effect/fireChip0001.nhtex", 0xc532d4b5c5756360 },
		{ "data/effect/Texture/Effect/fireWork0000.nhtex", 0x27c4a093b97f43e },
		{ "data/effect/Texture/Effect/fire_ball0000.nhtex", 0x2a47c6d2ea1a9b2b },
		{ "data/effect/Texture/Effect/gridtest_256-256.nhtex", 0x1b3af28ce5060aa5 },
		{ "data/effect/Texture/Effect/guard_loop0000.nhtex", 0xdb67d2c7fb1ea8fc },
		{ "data/effect/Texture/Effect/guard_ring0000.nhtex", 0xbcbd95829542870e },
		{ "data/effect/Texture/Effect/hijiri_spellC_Pilar.nhtex", 0x9e3b7b9526881d44 },
		{ "data/effect/Texture/Effect/hitParticle0000.nhtex", 0xc74237c14ee9443e },
		{ "data/effect/Texture/Effect/horizonWave0000.nhtex", 0x4f5d937ff598f052 },
		{ "data/effect/Texture/Effect/horizonWave0001.nhtex", 0xc918bf52c2d5514d },
		{ "data/effect/Texture/Effect/laserBaseA000.nhtex", 0xbb3f7a54dbf3db93 },
		{ "data/effect/Texture/Effect/laserBaseA001.nhtex", 0x3938bdb899fd16a2 },
		{ "data/effect/Texture/Effect/lightLineRng0000.nhtex", 0xde034f16032edd9f },
		{ "data/effect/Texture/Effect/lightning0000.nhtex", 0x6a963217ab486669 },
		{ "data/effect/Texture/Effect/lightning_map0000.nhtex", 0x246829defb959eb5 },
		{ "data/effect/Texture/Effect/lightning_map0001.nhtex", 0xa7fe32069ae7f664 },
		{ "data/effect/Texture/Effect/loop_auraRed0000.nhtex", 0xf6e462486845f543 },
		{ "data/effect/Texture/Effect/loop_fire0000.nhtex", 0xf0c39e5598b2e551 },
		{ "data/effect/Texture/Effect/loop_hex.nhtex", 0x66855c53f8686c5c },
		{ "data/effect/Texture/Effect/marisa_laylineA.nhtex", 0x3b6fe0c12c5dda4 },
		{ "data/effect/Texture/Effect/marisa_magicM.nhtex", 0xd4cc393115622c2d },
		{ "data/effect/Texture/Effect/marisa_missileFire0000.nhtex", 0x4cae68092f2af259 },
		{ "data/effect/Texture/Effect/marisa_star0000.nhtex", 0xf1fb701da947a00e },
		{ "data/effect/Texture/Effect/marisa_star0001.nhtex", 0x6df71a18e7fa7bd0 },
		{ "data/effect/Texture/Effect/mask_fadeBall0000.nhtex", 0x1a17a74cbb9b478 },
		{ "data/effect/Texture/Effect/mask_fadeBall0000_.nhtex", 0x35839a1467424470 },
		{ "data/effect/Texture/Effect/masterSpark0000.nhtex", 0x25ae259256df6b83 },
		{ "data/effect/Texture/Effect/miko_spellC_Branch.nhtex", 0xaac75f07d2ca1e26 },
		{ "data/effect/Texture/Effect/miko_spellC_Laser0000.nhtex", 0x35cf430a55e80cde },
		{ "data/effect/Texture/Effect/monoAuraA0000.nhtex", 0xe819fbeabc60d7fb },
		{ "data/effect/Texture/Effect/monoAuraA0001.nhtex", 0xa61b3ba8702e6d29 },
		{ "data/effect/Texture/Effect/reimu_amulet0000.nhtex", 0x1136090e147eea13 },
		{ "data/effect/Texture/Effect/ringA0000.nhtex", 0x1392100070607341 },
		{ "data/effect/Texture/Effect/shockWave0000.nhtex", 0x4205804d3ab7a36b },
		{ "data/effect/Texture/Effect/side_smash0000.nhtex", 0xa94c932964314335 },
		{ "data/effect/Texture/Effect/smoke0000.nhtex", 0xb0cdd109b6796f3b },
		{ "data/effect/Texture/Effect/smoke_mamizou0000.nhtex", 0x73425375ecedb72e },
		{ "data/effect/Texture/Effect/spark0000.nhtex", 0x7ad62a9b65242db7 },
		{ "data/effect/Texture/Effect/speedLineA0000.nhtex", 0xc4e0361307074ed4 },
		{ "data/effect/Texture/Effect/spellRing0001.nhtex", 0xd79d51fc6155da4e },
		{ "data/effect/Texture/Effect/spellRingMono0000.nhtex", 0x168820377a103da7 },
		{ "data/effect/Texture/Effect/splash0000.nhtex", 0xabfe0eb6e8d497e4 },
		{ "data/effect/Texture/Effect/spline0000.nhtex", 0xeb9f09d26518fb14 },
		{ "data/effect/Texture/Effect/sun_ring0000.nhtex", 0xb1f491638801fda1 },
		{ "data/effect/Texture/Effect/unzanThunder0000.nhtex", 0xacd059e0c34be85 },
		{ "data/effect/Texture/Effect/wall_hit0000.nhtex", 0x464e9d4527fab0d2 },
		{ "data/effect/Texture/Effect/yellowLaserA0000.nhtex", 0x3630ba82496fc5d9 },
		{ "data/effect/Texture/Effect/yellowLaserCore.nhtex", 0x81f2db314141544c },
		{ "data/effect/dashFront.eft", 0xa51606a6269b23d2 },
		{ "data/effect/dashLine.eft", 0xd9f3755733903e84 },
		{ "data/effect/dead_exp.eft", 0x9e655ebddad955ce },
		{ "data/effect/dead_exp2.eft", 0x327ade1bb11b032f },
		{ "data/effect/fireWork.eft", 0xe5e228afef964436 },
		{ "data/effect/futo_fireTower.eft", 0x6ef25de386459d5f },
		{ "data/effect/futo_tornade.eft", 0xb96b59f03ff84664 },
		{ "data/effect/graze.eft", 0xbbb02a17df0007c },
		{ "data/effect/guardCrash.eft", 0x74b2a525f91ea635 },
		{ "data/effect/guard_baria.eft", 0x55490f54fb020b },
		{ "data/effect/hijiri_chopInpact.eft", 0x3e267c63c4dc76ad },
		{ "data/effect/hijiri_chopPilar.eft", 0xb620707656b61ded },
		{ "data/effect/hijiri_eye.eft", 0xe9430093e33dddc0 },
		{ "data/effect/hijiri_fist.eft", 0x23566b4c28faf251 },
		{ "data/effect/hijiri_indr.eft", 0x4a3231723c77c6d1 },
		{ "data/effect/ichirin_hit.eft", 0x7ef66eec8f1bca74 },
		{ "data/effect/ichirin_spark.eft", 0x12e4cc5d0afd0246 },
		{ "data/effect/ichirin_sparkFlare.eft", 0xe4f5bf9f8f7d3021 },
		{ "data/effect/ichirin_sparkFlare15.eft", 0x6de702e68cc2a797 },
		{ "data/effect/ichirin_sparkFlare20.eft", 0xf6ef5b60f3e71616 },
		{ "data/effect/ichirin_sparkFlare25.eft", 0x4988fd21f48add7e },
		{ "data/effect/kokoro_fire.eft", 0xb8d7fdd5a0da8911 },
		{ "data/effect/kokoro_horizonWave.eft", 0x82dd4e95c6072d3c },
		{ "data/effect/kokoro_pose0000.eft", 0x33451719529e8005 },
		{ "data/effect/kokoro_pose0001.eft", 0xf5cf6c4eb5fa7bdd },
		{ "data/effect/kokoro_pose0002.eft", 0x7c71b6e30aa44024 },
		{ "data/effect/kokoro_pose0003.eft", 0x5e9c61db6724a856 },
		{ "data/effect/mamizou_smokeBurst.eft", 0x1497a62283b169b8 },
		{ "data/effect/mamizou_smokeBurstB.eft", 0x8ed0ed9a682ddc61 },
		{ "data/effect/mamizou_steam.eft", 0x417873644bc0397d },
		{ "data/effect/marisa_aside.eft", 0x133e1b191862c6e5 },
		{ "data/effect/marisa_asideB.eft", 0xab7fbd9c45f4dba8 },
		{ "data/effect/marisa_blazeStar.eft", 0xc6cf4c3fb80018c0 },
		{ "data/effect/marisa_chargeExp.eft", 0x989ac30e8c697540 },
		{ "data/effect/marisa_chargeShot.eft", 0x16dc690b425235f8 },
		{ "data/effect/marisa_chargeShot2.eft", 0x57e79415ec91f495 },
		{ "data/effect/marisa_finger.eft", 0x7fd20ca72cee80c2 },
		{ "data/effect/marisa_layLine.eft", 0xcfed134f8a999bea },
		{ "data/effect/marisa_pBomb.eft", 0x6b1e12abc64c51e7 },
		{ "data/effect/marisa_shotFire.eft", 0x849e5ccb113da59e },
		{ "data/effect/marisa_spark.eft", 0xd0053014c27f8b05 },
		{ "data/effect/marisa_sparkPre.eft", 0xc100141f5f653990 },
		{ "data/effect/marisa_strtHit.eft", 0x27feda3884c28e1a },
		{ "data/effect/mask_test.eft", 0xf96d8b64bbccc262 },
		{ "data/effect/miko_baria.eft", 0x842626d47c6cd9c2 },
		{ "data/effect/miko_dragonFire.eft", 0x486acf1f38621a77 },
		{ "data/effect/miko_dragonFireB.eft", 0xe299b3994c870e20 },
		{ "data/effect/miko_kanzen.eft", 0xe6baadb25af3cf9a },
		{ "data/effect/miko_kanzenB.eft", 0x970a1a8795e5dcf2 },
		{ "data/effect/miko_preRing.eft", 0xa5d23b7f30bdb312 },
		{ "data/effect/miko_sunRing.eft", 0xb2be5353beba8fc1 },
		{ "data/effect/miko_warp.eft", 0xa23638a5cc74116e },
		{ "data/effect/nitori_missileFire.eft", 0xfbdc29ce6042f1e0 },
		{ "data/effect/nitori_splash.eft", 0xf839745be3de5de },
		{ "data/effect/recover.eft", 0x16beb3fc16fd0d60 },
		{ "data/effect/reimu_box.eft", 0x4df1c3c8e7a412b3 },
		{ "data/effect/reimu_busterExp.eft", 0x96610d65bd2963b8 },
		{ "data/effect/reimu_fallkick.eft", 0x46a1021145d5402c },
		{ "data/effect/spellBreak.eft", 0x7a1dcf0a4cba0d23 },
		{ "data/effect/spellFlash.eft", 0x5122884fe2a99e96 },
		{ "data/effect/spell_break.eft", 0xa8d7e47f17b72d96 },
		{ "data/effect/test_grid.eft", 0x39dcb3c2a0930bde },
		{ "data/effect/wallHit.eft", 0xf840ebada9519a50 },
		{ "data/event/script/reimuB/stage3.pl", 0xd0e2820e72c4a55d },
		{ "data/plugin/se_chirp_helper.dll", 0x4159fd72ac1d488e },
		{ "data/plugin/se_hash.dll", 0xffb18a490a7cea2c },
		{ "data/plugin/se_information.dll", 0x2401230f11e94810 },
		{ "data/plugin/se_libact.dll", 0x274abec7889c3206 },
		{ "data/plugin/se_lobby.dll", 0xdff7c943f778da08 },
		{ "data/plugin/se_rpc.dll", 0x4417b39ea82d561 },
		{ "data/plugin/se_upnp.dll", 0x18f41f2cb79506de },
		{ "data/plugin/se_windowsize.dll", 0xbec0b6af1c65223c },
		{ "data/script/act_def.nut", 0x41c2737de6df92b9 },
		{ "data/script/actor.nut", 0x5fa120ccd2063404 },
		{ "data/script/background/bg01.nut", 0xab3e4d239dd7b61b },
		{ "data/script/background/bg02.nut", 0x1c0fe66fd177ad2b },
		{ "data/script/background/bg03.nut", 0xdbcf8e1ce611b635 },
		{ "data/script/background/bg04.nut", 0x20baf907a853bc44 },
		{ "data/script/background/bg06.nut", 0x1764406539e3f501 },
		{ "data/script/background/bg08.nut", 0xaa8b4409692bea86 },
		{ "data/script/background/bg18_min.nut", 0xd93424b7a54b3e8d },
		{ "data/script/background/bg19.nut", 0x955e48cf2162466f },
		{ "data/script/background/bg20.nut", 0x4c18521b203697e3 },
		{ "data/script/background/bg22.nut", 0xe713eaf383d95429 },
		{ "data/script/background/bg24.nut", 0x75ef9e376b9b973b },
		{ "data/script/background/bg29_min.nut", 0xc40e0ee323ef0232 },
		{ "data/script/background/bg_min.nut", 0x98bf020b4cced24f },
		{ "data/script/boot.nut", 0x34d73e823662b54b },
		{ "data/script/camera.nut", 0x309d498df9a4fe19 },
		{ "data/script/class.nut", 0xad8a38468395ee25 },
		{ "data/script/config.nut", 0x20afe674dd59fd1c },
		{ "data/script/const.nut", 0x23f07ddac192b2d7 },
		{ "data/script/const_key.nut", 0x6e97179cdab250f },
		{ "data/script/game.nut", 0x7f4c87380a94d464 },
		{ "data/script/game_arcade.nut", 0x6c8ec48397ddd50a },
		{ "data/script/game_load.nut", 0x72ef190effc9cab },
		{ "data/script/game_occult.nut", 0xe24c313438b8e99a },
		{ "data/script/game_scene.nut", 0x87685baeb4a67ed6 },
		{ "data/script/game_story.nut", 0x59b5f49df76c4589 },
		{ "data/script/game_tutorial.nut", 0xb4718eedab35a540 },
		{ "data/script/global.nut", 0x8fa8f5d8ee5dd3c4 },
		{ "data/script/hit.nut", 0x8b56d3bc023d577a },
		{ "data/script/input_command.nut", 0x5b32822808e34de6 },
		{ "data/script/loadFiles.nut", 0x9c7e4dcc41b04e88 },
		{ "data/script/practice.nut", 0x9edebff4ee64b61c },
		{ "data/script/render.nut", 0x130e980254d0109c },
		{ "data/script/replay.nut", 0xa1ddd0eaf6d8b2a1 },
		{ "data/script/savedata.nut", 0x94558631e71f80b3 },
		{ "data/script/story.nut", 0xb2557babac6ac167 },
		{ "data/script/version.nut", 0xc0e2fc1c5f503031 },
		{ "data/script/world.nut", 0x17dd1d6740515a72 },
		{ "data/se/common/865.wav", 0xb4985e0a47a7231f },
		{ "data/se/common/866.wav", 0x321ddc6e90fb603f },
		{ "data/se/common/867.wav", 0x74a7508fb742254d },
		{ "data/se/common/868.wav", 0x427b7d91daa2379a },
		{ "data/se/common/869.wav", 0xe2d25fc7316ef0bc },
		{ "data/se/koishi.csv", 0x7bbe72dee3f84b3a },
		{ "data/se/koishi/2465.wav", 0xceb3c205e150b38b },
		{ "data/se/se.csv", 0xb4f0ab62f4d27552 },
		{ "data/system/BattleStatus/BattleStatus.global.nut", 0xd561d0dd63c9c2a7 },
		{ "data/system/bg/bg.act", 0xa087a3adef5db92c },
		{ "data/system/char_select3/1/equip/c/000.png.csv", 0xbd912fa8a454ead9 },
		{ "data/system/char_select3/12/equip/a/000.png.csv", 0x5f627beabbc26ec8 },
		{ "data/system/char_select3/12/skill_list_a.dds", 0xb39dd89ca60fd69b },
		{ "data/system/char_select3/12/skill_list_b.dds", 0x8e906abace214259 },
		{ "data/system/char_select3/12/skill_list_c.dds", 0xf5c9679b7839d8bf },
		{ "data/system/char_select3/2/skill_list_a.dds", 0x3ab2e9c1df28f87d },
		{ "data/system/char_select3/2/skill_list_b.dds", 0x6501022e30863a12 },
		{ "data/system/char_select3/2/skill_list_c.dds", 0x5be62eaf72bbc527 },
		{ "data/system/char_select3/4/equip/b/000.png.csv", 0xeb5ca6547a75e72 },
		{ "data/system/char_select3/6/skill_list_a.dds", 0xa9cc6b383b304e78 },
		{ "data/system/char_select3/6/skill_list_b.dds", 0x7da57b1b821e3694 },
		{ "data/system/char_select3/6/skill_list_c.dds", 0xb92e94f6f76d5363 },
		{ "data/system/char_select3/7/color/000.png", 0xaed5057b6832c45c },
		{ "data/system/char_select3/9/equip/a/000.png.csv", 0xb7d881bd72c21019 },
		{ "data/system/char_select3/char_select3.act", 0x69a6112aab40638b },
		{ "data/system/char_select3/story_select.global.nut", 0x98cb8ebb6ccda2ca },
		{ "data/system/component/network.nut", 0x559c9bf8ff3b035a },
		{ "data/system/font/config_moji1.png", 0x5404eae0a97e05d2 },
		{ "data/system/menu_frame/menu_frame.act", 0xb27bf195b53cd114 },
		{ "data/system/menu_frame/window/base_popup_3line.png", 0x9315a83b36bce18e },
		{ "data/system/network_vs/base_lobby_select.png", 0xd7df8f957a2ff2ef },
		{ "data/system/network_vs/dialog.nut", 0x7abbceaba6315087 },
		{ "data/system/network_vs/lobby_lisence.png", 0x8d25fe35f117c8fc },
		{ "data/system/network_vs/network_vs.act", 0xae7a1a918bf0d033 },
		{ "data/system/network_vs/network_waiting.act", 0x90f1be3b8fc5938d },
		{ "data/system/practice/on_draw.nut", 0x2285060226bedb62 },
		{ "data/system/practice/practice.global.nut", 0x87c707d1bc9b2602 },
		{ "data/system/profile/profile.global.nut", 0xce2c2325adb9a242 },
		{ "data/system/profile/submenu/submenu.dds", 0xa33a2bb083720a65 },
		{ "data/system/profile/submenu/submenu2.dds", 0xa33a2bb083720a65 },
		{ "data/system/profile/submenu/submenu3.dds", 0xa33a2bb083720a65 },
		{ "data/system/replay_select/replay_select.act", 0xe1adcd0a12a1dae2 },
		{ "data/system/stage_select/stage_select.global.nut", 0xb6a4b1712d316bce },
		{ "data/system/title/Title.Debug.nut", 0x72f742733884a7f },
		{ "data/system/title/Title.act", 0x2fe891730b119ac8 },
		{ "data/system/title/update_stone.png", 0x92c31c20ecddf79a },
		{ "unk/6D71EAC2", 0x21e20c4b9da4d6d4 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th145";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th145b-test.pak";

	public ArchiveTh145Tests()
	{
		// Set up code page 932 (Shift-JIS)
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		Directory.CreateDirectory(ENTRIES_PATH);
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\th145b.pak")]
	public void ReadArchiveTh145(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.ULiL, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsAssignableFrom<TH135.TFPK>(archive);
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
	[InlineData($"{TEST_PATH}\\th145b.pak", true)]
	public async Task ReadArchiveTh145Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.ULiL, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.IsAssignableFrom<TH135.TFPK>(archive);
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
	public void WriteArchiveTh145(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.ULiL, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.ULiL, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh145Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.ULiL, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.ULiL, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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

	public void Dispose() => File.Delete(ARCHIVE_OUTPUT_PATH);
}
