using System.Collections.Frozen;
using System.IO.Hashing;
using System.Text;
using Touhou.Common;
using Touhou.Extraction.Tests.Utils;

namespace Touhou.Extraction.Tests;

public sealed class ArchiveTh105Tests : IDisposable
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "data/card/alice/card205.cv2", 0x4531d9482a692096 },
		{ "data/card/aya/card212.cv2", 0xc48dd68ebbdde70 },
		{ "data/card/komachi/card201.cv2", 0xc7ea021c812be710 },
		{ "data/card/sakuya/card202.cv2", 0xf7a88eca1a875915 },
		{ "data/card/suika/card206.cv2", 0x37d5d287d8b8cc28 },
		{ "data/card/youmu/card201.cv2", 0xc1705e7952583b02 },
		{ "data/character/alice/alice.pat", 0x646a35c5180295c },
		{ "data/character/alice/objectAk000.cv2", 0x491e8bfd32bcdc27 },
		{ "data/character/alice/objectAk001.cv2", 0xef21031d36b20305 },
		{ "data/character/alice/palette001.pal", 0xd3ad10179d1065f8 },
		{ "data/character/aya/aya.pat", 0x56b78b09653ccb51 },
		{ "data/character/iku/attackAb004.cv2", 0x413bacb0139fccd8 },
		{ "data/character/iku/attackAb005.cv2", 0x28b021e4b62b7b64 },
		{ "data/character/iku/attackAb006.cv2", 0x3f2c654de032d1da },
		{ "data/character/iku/attackBa000.cv2", 0x920b98403a4c254e },
		{ "data/character/iku/attackBa001.cv2", 0xe60220518526206a },
		{ "data/character/iku/attackBa004.cv2", 0xc3c66f99e3dbe6ec },
		{ "data/character/iku/attackBa005.cv2", 0x13050d5a6f47119 },
		{ "data/character/iku/attackBa006.cv2", 0x4176cbe85925d0e },
		{ "data/character/iku/attackBa007.cv2", 0x2dbe1416d1d077cd },
		{ "data/character/iku/attackBa009.cv2", 0xe2df6838d7431b1f },
		{ "data/character/iku/attackBa011.cv2", 0xce9c43cdf9c7d104 },
		{ "data/character/iku/attackBa012.cv2", 0xf4331132c5e06b72 },
		{ "data/character/iku/attackBa013.cv2", 0x2807412236be363c },
		{ "data/character/iku/attackBa014.cv2", 0xc4f9b5cec701bc2f },
		{ "data/character/iku/attackBa015.cv2", 0xf67e68a90222047e },
		{ "data/character/iku/attackBc000.cv2", 0x7dbecdab388b2106 },
		{ "data/character/iku/attackBc001.cv2", 0xbb3046e323c4096a },
		{ "data/character/iku/attackBc002.cv2", 0xb3ae7b127a69f714 },
		{ "data/character/iku/attackBc003.cv2", 0xec7bdb7b9a5b7490 },
		{ "data/character/iku/attackBc004.cv2", 0x88d8ee9f944e8ae8 },
		{ "data/character/iku/attackBd000.cv2", 0xeb3ceb85013a2b61 },
		{ "data/character/iku/attackBd001.cv2", 0x2109fd7807933bc4 },
		{ "data/character/iku/attackBd002.cv2", 0x2ec684ba866bd3c0 },
		{ "data/character/iku/attackBe000.cv2", 0xda21b58a11dbb67f },
		{ "data/character/iku/attackBe001.cv2", 0x52bc4d6aadd4657b },
		{ "data/character/iku/attackBe002.cv2", 0x5af218705f7d0824 },
		{ "data/character/iku/attackBe003.cv2", 0x3b79ebf0c96b6f76 },
		{ "data/character/iku/attackBe004.cv2", 0x8b1f3b15692d85ff },
		{ "data/character/iku/attackBe005.cv2", 0xaf5828bb9cf62610 },
		{ "data/character/iku/attackBe006.cv2", 0x4bf2c1111e7dc5a3 },
		{ "data/character/iku/attackBe007.cv2", 0x658afb52a332c3f6 },
		{ "data/character/iku/attackBf000.cv2", 0xbd9a8c0f100de0c4 },
		{ "data/character/iku/attackBf001.cv2", 0x7dd6bec2e430caa9 },
		{ "data/character/iku/attackBf002.cv2", 0x7d89bf7de91dff1a },
		{ "data/character/iku/attackBf003.cv2", 0x53bcbc940a8fef8 },
		{ "data/character/iku/attackBf004.cv2", 0xbfda2f8dc45e95f8 },
		{ "data/character/iku/iku.pat", 0x9901fee130d91360 },
		{ "data/character/iku/shotAa000.cv2", 0xd8e22e1ffd4b1a8 },
		{ "data/character/iku/shotAa001.cv2", 0x159b812fc6916be7 },
		{ "data/character/iku/shotAb001.cv2", 0xab5292981cd3d730 },
		{ "data/character/iku/shotAb010.cv2", 0xa00289fcf952f828 },
		{ "data/character/iku/shotAc001.cv2", 0x1f6f825d26d2ca25 },
		{ "data/character/iku/shotBa000.cv2", 0x6466e9547f76f78 },
		{ "data/character/iku/shotBa001.cv2", 0xcc48234297a00293 },
		{ "data/character/iku/shotBa002.cv2", 0x3001e16111cc018e },
		{ "data/character/iku/shotBa003.cv2", 0xe0b58f6b143fb05f },
		{ "data/character/iku/shotBa004.cv2", 0xd22ffeacf9fab012 },
		{ "data/character/iku/shotBa005.cv2", 0xb60412e89ae5371d },
		{ "data/character/iku/shotBa006.cv2", 0x3386d26f7127f4ad },
		{ "data/character/iku/shotBa007.cv2", 0x77f6eb0f0917262f },
		{ "data/character/iku/shotBa008.cv2", 0xbd7041f9b2160fea },
		{ "data/character/iku/shotBa009.cv2", 0x249c8b62e471b222 },
		{ "data/character/iku/shotBa010.cv2", 0x2d95ddb92a3f0c3a },
		{ "data/character/iku/spellAa000.cv2", 0x58b91076804921f7 },
		{ "data/character/iku/spellAa001.cv2", 0xb63419e7445f5c6e },
		{ "data/character/iku/spellAa002.cv2", 0xdd6ad6c79b830a2b },
		{ "data/character/iku/spellAa003.cv2", 0x987bc8988e84769a },
		{ "data/character/iku/spellAa006.cv2", 0x44ae5fff4fa6a22c },
		{ "data/character/iku/spellAa007.cv2", 0x965a72ae1fb3b37d },
		{ "data/character/iku/spellAa008.cv2", 0xda95fb59f7d210f },
		{ "data/character/iku/spellAa009.cv2", 0x8f7c3ef69f57ddba },
		{ "data/character/iku/spellAa010.cv2", 0xb0f3026a5516a3f8 },
		{ "data/character/iku/spellAa012.cv2", 0x9732a70f124a8d73 },
		{ "data/character/iku/spellAa015.cv2", 0xa4d4e3136cc45cec },
		{ "data/character/iku/spellAa016.cv2", 0xbad71e36c1581b16 },
		{ "data/character/iku/spellAa017.cv2", 0xa66920c32e83c590 },
		{ "data/character/iku/spellAa018.cv2", 0x9480b8a0acb06803 },
		{ "data/character/iku/spellBa000.cv2", 0x9cf1b0894d00d016 },
		{ "data/character/iku/spellBa001.cv2", 0xe70b4663d17dd1d9 },
		{ "data/character/iku/spellBb001.cv2", 0xe383af2eb0394ce3 },
		{ "data/character/iku/spellBb002.cv2", 0xbb864da217d1ff4d },
		{ "data/character/iku/spellBb003.cv2", 0x69b960fe40cc220a },
		{ "data/character/iku/spellBb004.cv2", 0xa730e294318d484f },
		{ "data/character/iku/spellBb005.cv2", 0x2259b0b840e5cc33 },
		{ "data/character/iku/spellBb006.cv2", 0x9e3b24190804670d },
		{ "data/character/iku/spellBb007.cv2", 0xc0637d292ce86b4e },
		{ "data/character/iku/spellBb008.cv2", 0xd1fb72c3d781f25e },
		{ "data/character/iku/spellBb009.cv2", 0xfe624498bf6eec74 },
		{ "data/character/iku/spellBb010.cv2", 0x9bba44a730418fd7 },
		{ "data/character/iku/spellBb011.cv2", 0xd3144c830741a07c },
		{ "data/character/iku/spellBb012.cv2", 0x45d128fde70f557c },
		{ "data/character/iku/spellCa000.cv2", 0x1f9c5fd0d3b02c2b },
		{ "data/character/iku/spellCa001.cv2", 0x744e359b6f016311 },
		{ "data/character/iku/spellCa002.cv2", 0xf01a4cd1e272175a },
		{ "data/character/iku/spellCa003.cv2", 0xbf981eab80027817 },
		{ "data/character/iku/spellCa004.cv2", 0xc8b3abc177822f91 },
		{ "data/character/iku/spellCa005.cv2", 0x248a51f620925497 },
		{ "data/character/iku/spellCa006.cv2", 0x1c0a14494f916a68 },
		{ "data/character/iku/spellCa007.cv2", 0x3f2dc04f5d1bf37e },
		{ "data/character/iku/spellCa008.cv2", 0xff73ea6ea7bad017 },
		{ "data/character/iku/spellCa009.cv2", 0x3d1d937c9673a157 },
		{ "data/character/iku/spellCa010.cv2", 0xd3e0dbeb09f08df1 },
		{ "data/character/iku/spellCa011.cv2", 0x4cba4cf9b6e0bf78 },
		{ "data/character/iku/spellCa012.cv2", 0x5dee233fb26cb4e2 },
		{ "data/character/iku/spellCa013.cv2", 0x50af20da2b35b24f },
		{ "data/character/iku/spellCa014.cv2", 0x8e25f5de03b30074 },
		{ "data/character/iku/spellCall000.cv2", 0x44d83855353c6e5b },
		{ "data/character/iku/spellCall001.cv2", 0x79ba22ab7abc742f },
		{ "data/character/iku/spellCall002.cv2", 0x341cc7086a248632 },
		{ "data/character/iku/spellCall003.cv2", 0x96fce5e9858380a4 },
		{ "data/character/iku/spellCall004.cv2", 0x2c77b7fa01b88c0d },
		{ "data/character/iku/spellCall005.cv2", 0x1034ac066c6f81e4 },
		{ "data/character/iku/spellCb000.cv2", 0x5ca86257dc53dfa2 },
		{ "data/character/iku/spellCb001.cv2", 0xd635d6e0972982f3 },
		{ "data/character/iku/spellCb002.cv2", 0x8a922a70abae8762 },
		{ "data/character/iku/spellCb003.cv2", 0xe77d35835e054bde },
		{ "data/character/iku/spellCb004.cv2", 0xa5c732387ff303e6 },
		{ "data/character/iku/spellCb005.cv2", 0xd2b199830ac2de67 },
		{ "data/character/iku/spellCb006.cv2", 0x891067eb610fde94 },
		{ "data/character/iku/spellCb007.cv2", 0x37a7a40da40932c5 },
		{ "data/character/iku/spellDa000.cv2", 0x559009a5baa250c9 },
		{ "data/character/iku/spellDa001.cv2", 0xb982c10990abf208 },
		{ "data/character/iku/spellDa002.cv2", 0xeccecbe452f1c822 },
		{ "data/character/iku/spellDa003.cv2", 0xf9ecae3427a03850 },
		{ "data/character/iku/spellDa004.cv2", 0x487f7d6e7b3b0e75 },
		{ "data/character/iku/spellDa007.cv2", 0xf53a8f2fefb42bf },
		{ "data/character/iku/spellDa008.cv2", 0x9e70b3da778f63ca },
		{ "data/character/iku/spellDa009.cv2", 0xb8be929ff17311c6 },
		{ "data/character/iku/spellDa011.cv2", 0xa631741cb7fc6c6a },
		{ "data/character/komachi/attackCc015.cv2", 0x2f6df5fc6f7cb8e7 },
		{ "data/character/komachi/attackCe000.cv2", 0x9e1721ab30ecaa66 },
		{ "data/character/komachi/attackCe001.cv2", 0xd9f0695367876977 },
		{ "data/character/komachi/attackCe002.cv2", 0x1e21a1148c8ba305 },
		{ "data/character/komachi/attackCe003.cv2", 0x5f7fc44a08cc3e45 },
		{ "data/character/komachi/attackCe004.cv2", 0x7c00be9d2f9f30ad },
		{ "data/character/komachi/attackCe005.cv2", 0xf55681a87c6ac405 },
		{ "data/character/komachi/attackCe006.cv2", 0xca9ffa07e4e587db },
		{ "data/character/komachi/attackCe007.cv2", 0x2d3a4da0f5023177 },
		{ "data/character/komachi/attackCe008.cv2", 0x19fda8e8212e824c },
		{ "data/character/komachi/attackCe009.cv2", 0x6318db04121fccaa },
		{ "data/character/komachi/attackCe010.cv2", 0x4d0d6be55add4b71 },
		{ "data/character/komachi/attackCe011.cv2", 0x3fe17f8efad6c6f3 },
		{ "data/character/komachi/attackCe012.cv2", 0xbe5710591fcb0cd3 },
		{ "data/character/komachi/attackCe013.cv2", 0x3aac1d5d467b6373 },
		{ "data/character/komachi/dashFront010.cv2", 0x291716f4f8d7f703 },
		{ "data/character/komachi/komachi.pat", 0xb5459ced70a2a920 },
		{ "data/character/komachi/walkBack000.cv2", 0x722929193397e79b },
		{ "data/character/komachi/walkBack001.cv2", 0x4de5079c3796cc01 },
		{ "data/character/komachi/walkBack002.cv2", 0xd64b3ab5b0ca0ef5 },
		{ "data/character/komachi/walkFront015.cv2", 0x4b11f04fb24d1581 },
		{ "data/character/marisa/marisa.pat", 0xbcae7596febab1bc },
		{ "data/character/patchouli/bulletAb000.cv2", 0x464ece4f5b34f335 },
		{ "data/character/patchouli/bulletAb001.cv2", 0x4fceb25b138bc14d },
		{ "data/character/patchouli/bulletEa000.cv2", 0x2e13f386629039d5 },
		{ "data/character/patchouli/bulletEa001.cv2", 0x24cfa9c75467d329 },
		{ "data/character/patchouli/bulletEa002.cv2", 0xf5d29d5704a62f73 },
		{ "data/character/patchouli/bulletEa003.cv2", 0xe8a36d442e61936f },
		{ "data/character/patchouli/bulletEa004.cv2", 0xa94dded27a26aca6 },
		{ "data/character/patchouli/bulletEa005.cv2", 0x4bd3f3782edbd2e4 },
		{ "data/character/patchouli/bulletEa006.cv2", 0x8234c0ce8edadbf7 },
		{ "data/character/patchouli/bulletEa007.cv2", 0xaa79343b586e32e },
		{ "data/character/patchouli/bulletEa008.cv2", 0x9e9ba1983527f366 },
		{ "data/character/patchouli/bulletEa009.cv2", 0x11584832818e7531 },
		{ "data/character/patchouli/bulletEa010.cv2", 0x9952e9cc6f80441f },
		{ "data/character/patchouli/bulletEa011.cv2", 0xe7649fd514c14e8c },
		{ "data/character/patchouli/bulletEa012.cv2", 0x2a014fe0850ab48d },
		{ "data/character/patchouli/bulletEa013.cv2", 0x3e1f12ebeacad188 },
		{ "data/character/patchouli/bulletEa014.cv2", 0x5698778bb9c4fc6a },
		{ "data/character/patchouli/bulletEa015.cv2", 0x3d39d63cb8edc33 },
		{ "data/character/patchouli/bulletEa016.cv2", 0x7b5e02b4eff29f9b },
		{ "data/character/patchouli/bulletEa017.cv2", 0x5f64dd887eb0a865 },
		{ "data/character/patchouli/bulletEa018.cv2", 0x2dfdde2500855315 },
		{ "data/character/patchouli/bulletEa019.cv2", 0x821cf4158e1216bd },
		{ "data/character/patchouli/bulletEa020.cv2", 0xe96830511fa4709a },
		{ "data/character/patchouli/bulletEa021.cv2", 0x4a6d577c76c28f5f },
		{ "data/character/patchouli/bulletEa022.cv2", 0x99a3ca1ca1ff6610 },
		{ "data/character/patchouli/bulletEa023.cv2", 0x881ec93dd47ba770 },
		{ "data/character/patchouli/bulletEa024.cv2", 0x68ed539a457e336e },
		{ "data/character/patchouli/bulletEa025.cv2", 0xda09536b1118d055 },
		{ "data/character/patchouli/bulletEa026.cv2", 0x411b20628830f519 },
		{ "data/character/patchouli/bulletEa027.cv2", 0xd95a58ebcb356adc },
		{ "data/character/patchouli/bulletEa028.cv2", 0x705a258206f26703 },
		{ "data/character/patchouli/bulletEa029.cv2", 0x25fae1b1da8b76b7 },
		{ "data/character/patchouli/bulletIc000.cv2", 0xb44e22bd8e6a26ee },
		{ "data/character/patchouli/bulletId000.cv2", 0x76a4e4976de3ec87 },
		{ "data/character/patchouli/patchouli.pat", 0xd0c8de170db84169 },
		{ "data/character/reimu/reimu.pat", 0xc4d9219615fd59b0 },
		{ "data/character/remilia/attackCb015.cv2", 0x5ec9d9b35067ed99 },
		{ "data/character/remilia/remilia.pat", 0x11548aa906c9bf9b },
		{ "data/character/sakuya/sakuya.pat", 0xcf178a87360c6974 },
		{ "data/character/suika/attackAe000.cv2", 0xffc56e1caca3b0e4 },
		{ "data/character/suika/attackAe001.cv2", 0x68070c18a4fee738 },
		{ "data/character/suika/attackAe002.cv2", 0x5cf778898fbffcc9 },
		{ "data/character/suika/attackAe003.cv2", 0x1b5e9178c364f554 },
		{ "data/character/suika/attackCe000.cv2", 0x2a73cf4134570660 },
		{ "data/character/suika/attackCe001.cv2", 0xab15031216a8d3d9 },
		{ "data/character/suika/attackCe002.cv2", 0xc431efc1fe97580e },
		{ "data/character/suika/attackCe003.cv2", 0xc9acf84c78a26dff },
		{ "data/character/suika/spellBulletDa000.cv2", 0xc3806a88a62433ca },
		{ "data/character/suika/suika.pat", 0xee0c0a471e8fc0f },
		{ "data/character/tenshi/palette001.pal", 0xd62776240e4eed7b },
		{ "data/character/tenshi/tenshi.pat", 0xc80025a417740b35 },
		{ "data/character/udonge/powerAura000.cv2", 0x1e9cc2986a39f7d2 },
		{ "data/character/udonge/udonge.pat", 0xe01ea395652023f1 },
		{ "data/character/youmu/youmu.pat", 0xad3bdc9c7941e57d },
		{ "data/character/yukari/yukari.pat", 0xc3721248a6578d18 },
		{ "data/character/yuyuko/yuyuko.pat", 0x4682160a62e6279e },
		{ "data/csv/alice/spellcard.cv1", 0x473f53db3c61380d },
		{ "data/csv/aya/spellcard.cv1", 0x5f3384e5d69804cf },
		{ "data/csv/common/spellcard.cv1", 0xfdf58f81f5608d60 },
		{ "data/csv/komachi/spellcard.cv1", 0x2137695de316de75 },
		{ "data/csv/remilia/storyspell.cv1", 0xbf29f76495c9e31d },
		{ "data/csv/sakuya/spellcard.cv1", 0x9431124a9a4f1d2d },
		{ "data/csv/suika/spellcard.cv1", 0x594e4134944e5ffa },
		{ "data/csv/udonge/spellcard.cv1", 0x598d112a45a5f00d },
		{ "data/csv/youmu/spellcard.cv1", 0x55b567392dff29b9 },
		{ "data/csv/yukari/spellcard.cv1", 0x567cb4626e36ba06 },
		{ "data/csv/yuyuko/spellcard.cv1", 0xb8f0719943c38da5 },
		{ "data/effect/cardCrash000.cv2", 0xd49d768cbb06e49d },
		{ "data/effect/cardCrash001.cv2", 0xb10fb773d5e0fba },
		{ "data/effect/cardCrash002.cv2", 0x149eb73e1f07ca25 },
		{ "data/effect/cardCrash003.cv2", 0x692c58841db54667 },
		{ "data/effect/cardCrash004.cv2", 0xf438f54e1daed444 },
		{ "data/effect/effect.pat", 0x92d3121e2cbea3c3 },
		{ "data/scenario/alice/004.cv0", 0x8052c17c485008cc },
		{ "data/scenario/marisa/ed.cv0", 0xcb4ed37fca807d6 },
		{ "data/scenario/sakuya/ed.cv0", 0xee5d90b9a935ba57 },
		{ "data/scenario/suika/ed.cv0", 0xc1e0ada94f6f429a },
		{ "data/scenario/suika/win.cv0", 0x2f4ce2ab83eaa6ca },
		{ "data/scenario/tenshi/000.cv0", 0xae0bf47cad17a8b2 },
		{ "data/scenario/tenshi/001.cv0", 0xc2fba58a09909d16 },
		{ "data/scenario/tenshi/002.cv0", 0xddd82e4d8f63d4ed },
		{ "data/scenario/tenshi/003.cv0", 0xd68815ca41d4815d },
		{ "data/scenario/tenshi/004.cv0", 0x5a4305cb67505438 },
		{ "data/scenario/tenshi/005.cv0", 0x9ae11a10290469c1 },
		{ "data/scenario/tenshi/006.cv0", 0x7898adcdb83d073b },
		{ "data/scenario/tenshi/007.cv0", 0x94c376dd846d5e1a },
		{ "data/scenario/yukari/005.cv0", 0xc262ddee156927a6 },
		{ "data/scene/select/bg/bg_pict10.cv2", 0x90ee1d8fc08a3d4b },
		{ "data/se/057.cv3", 0x66f46266d87855d0 },
		{ "data/se/058.cv3", 0x2fc463692d98cbcf },
		{ "data/se/iku/012.cv3", 0xd86058a967f1db },
		{ "data/se/udonge/056.cv3", 0x2e5d05ed0d7965d8 },
		{ "data/se/yukari/004.cv3", 0x9ed2869e855d9c10 },
		{ "data/se/yukari/005.cv3", 0x85a7919c9e2709b5 },
		{ "data/stand/cutin/sakuya.cv2", 0xdcdaa1903bf9a296 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th105";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries";
	private const string ARCHIVE_OUTPUT_PATH = $"{TEST_PATH}\\th105c-test.dat";

	public ArchiveTh105Tests()
	{
		// Set up code page 932 (Shift-JIS)
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		Directory.CreateDirectory(ENTRIES_PATH);
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\th105c.dat")]
	public void ReadArchiveTh105(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.SWR, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th105c.dat", true)]
	public async Task ReadArchiveTh105Async(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.SWR, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	public void WriteArchiveTh105(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		Archive.Create(Game.SWR, ARCHIVE_OUTPUT_PATH, entriesPath);

		using Archive archive = Archive.Read(Game.SWR, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.OpenReadFileStreamOptions));

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
	public async Task WriteArchiveTh105Async(string entriesPath)
	{
		string[] entryPaths = Guard.FailIfDirectoryEmpty(entriesPath);

		await Archive.CreateAsync(Game.SWR, ARCHIVE_OUTPUT_PATH, entriesPath);

		await using Archive archive = await Archive.ReadAsync(Game.SWR, new FileStream(ARCHIVE_OUTPUT_PATH, FileUtils.AsyncOpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th105a.dat", "data/character/alice/stand/惑.cv2", 0xb2e86e1c8f8174b)]
	public void ReadEntryWithJapaneseNameTh105(string path, string entryName, ulong entryDataHash)
	{
		Guard.FailIfFileDoesNotExist(path);

		using Archive archive = Archive.Read(Game.SWR, new FileStream(path, FileUtils.OpenReadFileStreamOptions));

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
	[InlineData($"{TEST_PATH}\\th105a.dat", "data/character/alice/stand/惑.cv2", 0xb2e86e1c8f8174b, true)]
	public async Task ReadEntryWithJapaneseNameTh105Async(string path, string entryName, ulong entryDataHash, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using Archive archive = await Archive.ReadAsync(Game.SWR, new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

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
	[InlineData($"{ENTRIES_PATH}-jp", "data/character/alice/stand/惑.cv2", 0xb2e86e1c8f8174b)]
	public void WriteEntryWithJapaneseNameTh105(string entriesPath, string entryName, ulong entryDataHash)
	{
		using MemoryStream outputStream = new();
		Archive.Create(Game.SWR, outputStream, entriesPath);

		using MemoryStream inputStream = new(outputStream.GetBuffer(), writable: false);
		using Archive archive = Archive.Read(Game.SWR, inputStream);

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
	[InlineData($"{ENTRIES_PATH}-jp", "data/character/alice/stand/惑.cv2", 0xb2e86e1c8f8174b)]
	public async Task WriteEntryWithJapaneseNameTh105Async(string entriesPath, string entryName, ulong entryDataHash)
	{
		await using MemoryStream outputStream = new();
		await Archive.CreateAsync(Game.SWR, outputStream, entriesPath);

		await using MemoryStream inputStream = new(outputStream.GetBuffer(), writable: false);
		await using Archive archive = await Archive.ReadAsync(Game.SWR, inputStream);

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
