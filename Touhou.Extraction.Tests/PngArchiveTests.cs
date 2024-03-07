﻿using System.Collections.Frozen;
using System.IO.Hashing;
using Touhou.Extraction.Tests.Utils;
using Touhou.Extraction.TH75;

namespace Touhou.Extraction.Tests;

public sealed class PngArchiveTests
{
	private static readonly FrozenDictionary<string, ulong> s_entryHashes = new Dictionary<string, ulong>()
	{
		{ "00.png", 0xce50d4f8bf5608d8 },
		{ "01.png", 0xc7456269f234b7f5 },
		{ "02.png", 0xe5a2bd2b293a8329 },
		{ "03.png", 0x1198a4509efa6a36 },
		{ "04.png", 0x568172a8fa138edf },
		{ "05.png", 0x9c8a7f948b6821d0 },
		{ "06.png", 0xc8fbe2e86cc7610f },
		{ "07.png", 0x9050e75cdb515881 },
		{ "08.png", 0x69d47504ee5303fb },
		{ "09.png", 0xb7a2fa555434bbb9 },
		{ "10.png", 0x79181de1ecd95cc3 },
		{ "11.png", 0xd359694e16b888b0 },
		{ "12.png", 0x290f5f7d00accc6c },
		{ "13.png", 0x483f06059603b704 },
		{ "14.png", 0xbf24d3552257de32 },
		{ "15.png", 0xd55ce07344da985 },
		{ "16.png", 0x362912e751ca5ef6 },
		{ "17.png", 0xe085cf142bce4b6 },
		{ "18.png", 0xdc3eee1456bc6f9e },
		{ "19.png", 0xff40b1622412dbac },
		{ "20.png", 0x969f2a8c91b75f48 },
		{ "21.png", 0xfbe4ab1b1fee852b },
		{ "22.png", 0x65bc42d4aaf92946 },
		{ "23.png", 0xd481f1712bccabe9 },
		{ "24.png", 0x245eaa399395e0f0 },
		{ "25.png", 0xe56d80e788aea5f9 },
		{ "26.png", 0x4df5b1f3522490d },
		{ "27.png", 0xfea7dd521ca0e173 },
		{ "28.png", 0x6b07ffb062d729c5 },
		{ "29.png", 0x65d3cab434dc92de },
		{ "30.png", 0xc861fac26c4da376 },
		{ "31.png", 0x9fef88244781334c },
		{ "32.png", 0x6bcca51cd407311f },
		{ "33.png", 0xbccdffae258a976a },
		{ "34.png", 0x7b4e184fc7dbb62a },
		{ "35.png", 0x84ef5b64d0c0598d },
		{ "36.png", 0xf6602de7216b0819 },
		{ "37.png", 0xfeda04fc07b51dcb },
		{ "38.png", 0x979171d4a5ddd79d },
		{ "39.png", 0xd8c10cb75c6ec3c7 },
		{ "40.png", 0x2b99d35e6508e873 },
		{ "41.png", 0xb0936c0a4d96ce10 },
		{ "42.png", 0x9694bc992ed2e783 },
		{ "43.png", 0x9de9b0fdf7a161fb },
		{ "44.png", 0x17ba2909bd4f3efa },
		{ "45.png", 0x7503441295728284 },
		{ "46.png", 0x1634e27225e39dd0 },
		{ "47.png", 0xb626b28de9dd8bec },
		{ "48.png", 0xf797e772a8f7ce1a },
		{ "49.png", 0x7133c89d17aee129 },
		{ "50.png", 0x89e3728c97f990a5 },
		{ "51.png", 0xf7a1b1c9ca5f74a9 },
		{ "52.png", 0x853614114fe218d8 },
		{ "53.png", 0x657a52ffb659d9a6 },
		{ "54.png", 0x87037f014b94ff43 },
		{ "55.png", 0xa69e0e24dc24e4d0 },
		{ "56.png", 0x2e483a50b7ec9480 },
		{ "57.png", 0xa73983d5db8b12b5 },
		{ "58.png", 0xced296b8c6d70c23 },
		{ "59.png", 0x32c8f271a27100f2 },
		{ "60.png", 0x4b39a9af2467fce },
		{ "61.png", 0xdcbe50573811df7 },
		{ "62.png", 0x8cead9800e7da940 },
		{ "63.png", 0xce82e8f0cd5bd4b0 },
		{ "64.png", 0x2f15fff78dc3f76e },
		{ "65.png", 0xc7d78f4e4dcada22 },
		{ "66.png", 0xaedf611bccb53404 },
		{ "67.png", 0xaaab71d50bb593ca },
		{ "68.png", 0x9c31c82c65456da6 },
		{ "69.png", 0x4aedab4826542a2d },
		{ "70.png", 0x79828d3eaf937d89 },
		{ "71.png", 0x530a15b1070cd18a },
		{ "72.png", 0xdb2dae344faa45a0 },
		{ "73.png", 0x5065fcf65f2e03c3 },
		{ "74.png", 0x52ef9895bd7ecd8b },
		{ "75.png", 0xe33c207ccb968473 },
		{ "76.png", 0x4afc4ad25afe2584 },
		{ "77.png", 0x80f7073fc3714fef },
		{ "78.png", 0x3bdb970fcc33459f },
		{ "79.png", 0xf65436b446fa5284 },
		{ "80.png", 0xdb27f3d1752a275f },
		{ "81.png", 0x515a3d13c67ab274 },
		{ "82.png", 0xc21d66fcba42d137 },
		{ "83.png", 0x520ea71fda8dd92 },
		{ "84.png", 0x594d6526ba865caf },
		{ "85.png", 0x711dc52a29ae381 },
		{ "86.png", 0x9e504690854d34b0 },
		{ "87.png", 0xbc56a64a7e4e4914 },
		{ "88.png", 0x6922952127b9cc33 },
		{ "89.png", 0x166e2fe9b2449e88 },
		{ "90.png", 0xcdfc1c3a0b5f5efc },
		{ "91.png", 0x14f7fe54b56cbbc7 },
		{ "92.png", 0x109b5b80e15d01f9 },
		{ "93.png", 0xc10e966dc04fe229 },
		{ "94.png", 0xc58071dbd6db7853 },
		{ "95.png", 0x6297451cc032c4f8 },
		{ "96.png", 0xc434732bcc1663b1 },
		{ "97.png", 0x3cd0c9f1fc176bde },
		{ "98.png", 0xb5a54bff1f8c6863 },
		{ "99.png", 0x22ccb5bfcdc18b98 },
		{ "100.png", 0x593e41b74a57a7f4 },
		{ "101.png", 0xbc85b69fc4db16d5 },
		{ "102.png", 0xa5fcd323ff6a3dac },
		{ "103.png", 0x93aaf677de226038 },
		{ "104.png", 0xce818a992d55e0db },
		{ "105.png", 0xce818a992d55e0db },
		{ "106.png", 0xce818a992d55e0db },
		{ "107.png", 0xd2aaceeef06156ee },
		{ "108.png", 0x3575a15279675b49 },
		{ "109.png", 0x3575a15279675b49 },
		{ "110.png", 0x31e211b981117259 },
		{ "111.png", 0xada72aea854c3e85 },
		{ "112.png", 0x9b1e80034450431 },
		{ "113.png", 0xca4a2ffca5d350e1 },
		{ "114.png", 0xd1de583d1f1d4a54 },
		{ "115.png", 0x6181c7f25d434590 },
		{ "116.png", 0xb37658937029b5ce },
		{ "117.png", 0xbbe9f21a5b41a4ae },
		{ "118.png", 0x3e84d86a5402e3c0 },
		{ "119.png", 0x294b5593112378d9 },
		{ "120.png", 0x6b47f9c85796fb0e },
		{ "121.png", 0xa94fd5edbae8b64c },
		{ "122.png", 0x66dddc4738d04413 },
		{ "123.png", 0x27b8d2f91dc3207c },
		{ "124.png", 0x8431cd9186e56678 },
		{ "125.png", 0x5bd5e3c57101f985 },
		{ "126.png", 0x69031335b43ea887 },
		{ "127.png", 0x30fcc532d14c72ef },
		{ "128.png", 0x46735affb718c423 },
		{ "129.png", 0x12ac03036da70674 },
		{ "130.png", 0x6750c259c9e41143 },
		{ "131.png", 0x45fd20bf7d40ca54 },
		{ "132.png", 0xa7bd1cdbf1637537 },
		{ "133.png", 0xe5015b2f259ed982 },
		{ "134.png", 0xd626e9684312e3b1 },
		{ "135.png", 0xa2c166bda277a435 },
		{ "136.png", 0xe243faa702e7c617 },
		{ "137.png", 0x41897f0cc6b7ad48 },
		{ "138.png", 0xba9bc111d9ffb4ab },
		{ "139.png", 0x1c20271237db5546 },
		{ "140.png", 0xe8fdcbd80f54d484 },
		{ "141.png", 0x1202f957f91a9f0b },
		{ "142.png", 0xb65a12baa28b96b6 },
		{ "143.png", 0x88ec6e0e73651f1d },
		{ "144.png", 0x71250683d315e53e },
		{ "145.png", 0x2ec423c295be3c0a },
		{ "146.png", 0xec21c518a76154af },
		{ "147.png", 0x55b58b39d5ead5de },
		{ "148.png", 0x55b58b39d5ead5de },
		{ "149.png", 0x55b58b39d5ead5de },
		{ "150.png", 0x55b58b39d5ead5de },
		{ "151.png", 0x55b58b39d5ead5de },
		{ "152.png", 0x61d73f47f70b5ea },
		{ "153.png", 0x3d648f9c29817853 },
		{ "154.png", 0xc6205860cb52120f },
		{ "155.png", 0x619afd5f7d56b56e },
		{ "156.png", 0xf3c542656f81ca1e },
		{ "157.png", 0x9e08882dd63d21c3 },
		{ "158.png", 0x1aaf1d447d45b205 },
		{ "159.png", 0x44495d33def1147c },
		{ "160.png", 0xb6b783fe1d965d8c },
		{ "161.png", 0xabbad1c5249c55bf },
		{ "162.png", 0x4349d4411d41fcb8 },
		{ "163.png", 0xef314089ae31dd48 },
		{ "164.png", 0xe2d8b918f47fde5f },
		{ "165.png", 0xe3c00d58e3b74cd1 },
		{ "166.png", 0xe4148635e84765a4 },
		{ "167.png", 0xfe7d3c2c55910629 },
		{ "168.png", 0x3dcc69e84202d99d },
		{ "169.png", 0xcf3ca5780f824ab7 },
		{ "170.png", 0x35e443faf881ed34 },
		{ "171.png", 0xcb688112668d2282 },
		{ "172.png", 0xfba59319da2e693f },
		{ "173.png", 0x3969887b8d7c0661 },
		{ "174.png", 0x7e940d5ae5c76113 },
		{ "175.png", 0x385089b7e6328448 },
		{ "176.png", 0x295497e6d732f995 },
		{ "177.png", 0xef792b10a634aadc },
		{ "178.png", 0xbccc18b2ed7030f2 },
		{ "179.png", 0x9efcc1bcc0cb9c11 },
		{ "180.png", 0x25fdd6f6e82b2545 },
		{ "181.png", 0x3ec174b45eccf3f },
		{ "182.png", 0x5f6343b441447df5 },
		{ "183.png", 0x79b35fe9fd6bf8c8 },
		{ "184.png", 0x79b35fe9fd6bf8c8 },
		{ "185.png", 0xab5c15a6710d4962 },
		{ "186.png", 0xaa75f6674c18d027 },
		{ "187.png", 0xa9705c64371f9b5b },
		{ "188.png", 0x4f68a3090a49ddba },
		{ "189.png", 0x33b770ec60ce74ed },
		{ "190.png", 0x92ee1b0f5548c0a0 },
		{ "191.png", 0xaab5b5a53a6dfef7 },
		{ "192.png", 0x578454c28fbbf117 },
		{ "193.png", 0x6781d43ce9f3ded },
		{ "194.png", 0xdd2b70b1ec11de60 },
		{ "195.png", 0xd147c6302b8c555 },
		{ "196.png", 0xa33f8b055f758c97 },
		{ "197.png", 0xdab0f5771f8992b4 },
		{ "198.png", 0xda3679ef2b8fa6b2 },
		{ "199.png", 0x4b29ee7ee92ae9f5 },
		{ "200.png", 0x798fd25f6d06c3e },
		{ "201.png", 0xd93cf3e03881b579 },
		{ "202.png", 0x23e1766e23e11ff5 },
		{ "203.png", 0x2d669c23160992ed },
		{ "204.png", 0x2f1337d8e04d3f26 },
		{ "205.png", 0x86cdce62f805bafe },
		{ "206.png", 0xf5ab90711ca2f205 },
		{ "207.png", 0xdfa192ff6f2f09ac },
		{ "208.png", 0xd93cf3e03881b579 },
		{ "209.png", 0xd93cf3e03881b579 },
		{ "210.png", 0x5e09398a1f196064 },
		{ "211.png", 0x3bf0ad11adc5c6b1 },
		{ "212.png", 0xf304825dcdd2be3d },
		{ "213.png", 0xd182de7e6734d3eb },
		{ "214.png", 0xca3a1482ec810121 },
		{ "215.png", 0x688e22c824b21e31 },
		{ "216.png", 0xb57bd8619801ecf },
		{ "217.png", 0xd89d5b2464b1ea26 },
		{ "218.png", 0x71dd34031eaf5f0f },
		{ "219.png", 0x37be0bfd61356140 },
		{ "220.png", 0x6b4aee0d7b7a85b0 },
		{ "221.png", 0xa8a43adb3cf8cd88 },
		{ "222.png", 0x5cb695cf0dbc4717 },
		{ "223.png", 0xd5fabf8d79f685b6 },
		{ "224.png", 0xa60d97abe9b2921d },
		{ "225.png", 0x8d4ce2ca4a5f18e1 },
		{ "226.png", 0xb99345ee53d5984e },
		{ "227.png", 0xf8100accf8cc99ef },
		{ "228.png", 0x4d846c18bfc34d9 },
		{ "229.png", 0xfe64abeec3219aa8 },
		{ "230.png", 0x9d8f685ed81cca94 },
		{ "231.png", 0x826f7ca619f398c0 },
		{ "232.png", 0xb9bfff76b5a072d5 },
		{ "233.png", 0xb6a4168a674599ec },
		{ "234.png", 0x7a335faec74bc40d },
		{ "235.png", 0xe7fed8b81b142018 },
		{ "236.png", 0x70bde062c1091f76 },
		{ "237.png", 0x830d5c2f37e640e6 },
		{ "238.png", 0x10302fed7efe73d9 },
		{ "239.png", 0xa3183bd41aeac27 },
		{ "240.png", 0x68d3e19d99d23f1f },
		{ "241.png", 0x507cb4cf79ceb62b },
		{ "242.png", 0xaf567cc770504d44 },
		{ "243.png", 0x88779527c953c194 },
		{ "244.png", 0x47a1ee3239212e1d },
		{ "245.png", 0xef4e0a2af95136f8 },
		{ "246.png", 0x51f9648075449060 },
		{ "247.png", 0x7a77a898dc7932a0 },
		{ "248.png", 0xde3c95cccecdfcb7 },
		{ "249.png", 0x840cbe3bb0aebecb },
		{ "250.png", 0x5c0c4be1b90868f6 },
		{ "251.png", 0x4b090a950091cce6 },
		{ "252.png", 0x2041a98527ecb35c },
		{ "253.png", 0x7f7b157d9d7e0ed4 },
		{ "254.png", 0x28b311ca7dacb212 },
		{ "255.png", 0xbcfa4de0e03f8a96 },
		{ "256.png", 0x8f4c5b9732517cd9 },
		{ "257.png", 0x6441da373de2b637 },
		{ "258.png", 0x38ab39fcdb977147 },
		{ "259.png", 0x3173ddb6b37f5f },
		{ "260.png", 0xb16cad0856c8cbfe },
		{ "261.png", 0x51f020bd24a291e8 },
		{ "262.png", 0x9d4b735680b7bb5f },
		{ "263.png", 0xf4e8bf4644488abc },
		{ "264.png", 0xd0e5255d9c971a19 },
		{ "265.png", 0xddfe5b2320380415 },
		{ "266.png", 0x5e45b1a7ceff4dbf },
		{ "267.png", 0x8da2f7b33fbc5ca1 },
		{ "268.png", 0xddfe5b2320380415 },
		{ "269.png", 0xc29078d0e1fe9e1a },
		{ "270.png", 0xd0e5255d9c971a19 },
		{ "271.png", 0xc7409c3f2c358830 },
		{ "272.png", 0x3234a56db772bceb },
		{ "273.png", 0xd0e5255d9c971a19 },
		{ "274.png", 0x5e45b1a7ceff4dbf },
		{ "275.png", 0x636c02dfa25c6554 },
		{ "276.png", 0xaf9e799e7b8d0cfa },
		{ "277.png", 0xa0b991cd2ce2d816 },
		{ "278.png", 0x948dbd8e55421f32 },
		{ "279.png", 0x8802bb0016d1d6b3 },
		{ "280.png", 0x81b7397bed4ca3d2 },
		{ "281.png", 0x3485c60ca9c5a0a },
		{ "282.png", 0xcf8499607aac1a62 },
		{ "283.png", 0x345132d50b081a0f },
		{ "284.png", 0xba4c3e90f2bf885d },
		{ "285.png", 0x80817742e86b2482 },
		{ "286.png", 0xdfd4e8979e0372ea },
		{ "287.png", 0x8bae980933083fd1 },
		{ "288.png", 0x80b7c773cc55e6f6 },
		{ "289.png", 0x310027488ea04cf7 },
		{ "290.png", 0x34875758a4ec9a6a },
		{ "291.png", 0xa24d58afea22a59d },
		{ "292.png", 0xb152bde0a6d318d },
		{ "293.png", 0xdc22c8b85f8c5896 },
		{ "294.png", 0x4a38d00a1aa51cf2 },
		{ "295.png", 0x53944561d0ca72b7 },
		{ "296.png", 0xb54da665bd65ab69 },
		{ "297.png", 0xc1c49588ea70fcb4 },
		{ "298.png", 0x93efdd6f533df93f },
		{ "299.png", 0x1cd17f2b31880c57 },
		{ "300.png", 0xc854c211d4a55517 },
		{ "301.png", 0x960146c786adc299 },
		{ "302.png", 0x4d5c601a827b51fb },
		{ "303.png", 0xe1267d6e9b2a6469 },
		{ "304.png", 0xa1e5f062f511f0da },
		{ "305.png", 0xe90c3c6618182121 },
		{ "306.png", 0x4c929ed866719931 },
		{ "307.png", 0x21b16a078f4b7c1d },
		{ "308.png", 0xe59adf325216f7e0 },
		{ "309.png", 0xf480d8b8a2d0b4c4 },
		{ "310.png", 0xac772367bb9ce7bd },
		{ "311.png", 0xbe645b5f151070f1 },
		{ "312.png", 0x4b42a4b0b74eda92 },
		{ "313.png", 0x2e8eea7464e7dc1a },
		{ "314.png", 0x14328626a5f3ecf4 },
		{ "315.png", 0x8567f9b531667a00 },
		{ "316.png", 0xe2bca1deb13d955d },
		{ "317.png", 0x14328626a5f3ecf4 },
		{ "318.png", 0x57ac36d42fab1cd6 },
		{ "319.png", 0x46ad316d648e3fff },
		{ "320.png", 0x63936ba8b0a26c3a },
		{ "321.png", 0x42301044547f4bbe },
		{ "322.png", 0x1894dfe2ec7a37c6 },
		{ "323.png", 0x675ec0e18f1be9c8 },
		{ "324.png", 0xd9e77bd70e590d1c },
		{ "325.png", 0x14328626a5f3ecf4 },
		{ "326.png", 0xac08e9f9130f6856 },
		{ "327.png", 0x6f12b65ff1e890cb },
		{ "328.png", 0x2846ff3e9c671b59 },
		{ "329.png", 0xc42b93a9c04b8aa2 },
		{ "330.png", 0x145fbaa3987ae655 },
		{ "331.png", 0xdce5426fd2e77a07 },
		{ "332.png", 0x892228b27c2d3724 },
		{ "333.png", 0xb41b58bc3c8576b },
		{ "334.png", 0xe6e776c97bfae954 },
		{ "335.png", 0xeeefda6b2f4c06a },
		{ "336.png", 0x6460064ee455955a },
		{ "337.png", 0x62dcf042b0195b8a },
		{ "338.png", 0xa671502d55afc981 },
		{ "339.png", 0x32ea435c99a50eca },
		{ "340.png", 0x9b0c2ff747211a86 },
		{ "341.png", 0xccd9b3edce39ce26 },
		{ "342.png", 0x8e20c5a6ebc1a452 },
		{ "343.png", 0x7759de6b4acacc4c },
		{ "344.png", 0x31fe258e1c1e6e17 },
		{ "345.png", 0xa339382358906aea },
		{ "346.png", 0xccd93a5f8d7f9ab4 },
		{ "347.png", 0xaedb98bf66d8cf3c },
		{ "348.png", 0x35c86742447ec777 },
		{ "349.png", 0x6e8ecc2c246f1cc7 },
		{ "350.png", 0x15baf4d5268aabd3 },
		{ "351.png", 0x2cae56fdd0ee9344 },
		{ "352.png", 0x559848699da16a43 },
		{ "353.png", 0xdb32a85b9e393a1b },
		{ "354.png", 0x4d58efabbde8e20b },
		{ "355.png", 0xfbc18cb7e61a5dfb },
		{ "356.png", 0x3d579902d6e801bf },
		{ "357.png", 0x1ebee24bcb8b05b3 },
		{ "358.png", 0xb47729973abc5965 },
		{ "359.png", 0x75fb8a9a4f7a9103 },
		{ "360.png", 0xcc1ce818946206ad },
		{ "361.png", 0xa43835f92f5ed007 },
		{ "362.png", 0x2ec87e1dbe555cf7 },
		{ "363.png", 0x1844fbfdfcd34d28 },
		{ "364.png", 0xf6c76731ea66d68 },
		{ "365.png", 0xe2c989889d8fac81 },
		{ "366.png", 0x9905a6cfb2e3bb3 },
		{ "367.png", 0xe0b76b1692715de5 },
		{ "368.png", 0xeb08a18d4e38f893 },
		{ "369.png", 0xd752433e7d1daaf6 },
		{ "370.png", 0xdd15acca44e74482 },
		{ "371.png", 0x8051a46b1cf9d45e },
		{ "372.png", 0x772b83c203af95c2 },
		{ "373.png", 0x8a8bb0009079d70a },
		{ "374.png", 0x62a5539fb46ca68c },
		{ "375.png", 0x82be5b517393a2d2 },
		{ "376.png", 0xab95ea64616f8476 },
		{ "377.png", 0xff15e9d17b162351 },
		{ "378.png", 0x1f1b5622676e16fa },
		{ "379.png", 0x7a06d3845a08f0fb },
		{ "380.png", 0xc0b7f15f8d3b6afb },
		{ "381.png", 0xfce0dee1428d18ee },
		{ "382.png", 0xdd8005e2923f6bd5 },
		{ "383.png", 0x29433b46ecd5a1b0 },
		{ "384.png", 0x9e2287b589d0bcca },
		{ "385.png", 0xae11ff6291163f1a },
		{ "386.png", 0xac9ec12938059785 },
		{ "387.png", 0x4344695b7b5f6f72 },
		{ "388.png", 0x8a8d2adb0af944f2 },
		{ "389.png", 0x55daeac0b2ab1a4f },
		{ "390.png", 0xfda66165650b819 },
		{ "391.png", 0x440fd71b50e409ff },
		{ "392.png", 0x734ac3db1c9fe9b3 },
		{ "393.png", 0x36356e493dd4251b },
		{ "394.png", 0x5f79820e59fac146 },
		{ "395.png", 0x7cd34178b6163be1 },
		{ "396.png", 0x5f36622f2a60c634 },
		{ "397.png", 0x27cff90458ec35c9 },
		{ "398.png", 0x1a21d6fd349f717b },
		{ "399.png", 0x59a837450a4f42ec },
		{ "400.png", 0xcf8a469d8e9d3aaf },
		{ "401.png", 0x48721a0cb87308f3 },
		{ "402.png", 0xc806557a61a8ba5c },
		{ "403.png", 0x888c4eba6c5b88b8 },
		{ "404.png", 0x87455abffcd21c0c },
		{ "405.png", 0xfbd5ffc93e464c37 },
		{ "406.png", 0x526221bbf6b37147 },
		{ "407.png", 0x59053e59e858bf5b },
		{ "408.png", 0x5b1658b39e18fb7 },
		{ "409.png", 0xf1702bd3a7b6b08e },
		{ "410.png", 0xc01320566af8288f },
		{ "411.png", 0xa0bbab11b2fecea6 },
		{ "412.png", 0x226fd578ec829f0d },
		{ "413.png", 0x7dfd8ed1630495d6 },
		{ "414.png", 0x925942657cdc7c4 },
		{ "415.png", 0xd7fbe25d9085cc6d },
		{ "416.png", 0xc05c19c3a8c118d9 },
		{ "417.png", 0x8e6417702fe4e759 },
		{ "418.png", 0xb8957a3efbaca80 },
		{ "419.png", 0x43408b2ccff6ba66 },
		{ "420.png", 0x766b8ceacca86375 },
		{ "421.png", 0xbc29e2c9f2741031 },
		{ "422.png", 0x1b9690ef1fa36edf },
		{ "423.png", 0x93e833cdb5634d78 },
		{ "424.png", 0x4806de11a87935fe },
		{ "425.png", 0x7ff9dab113c30c90 },
		{ "426.png", 0x454adad025dd0766 },
		{ "427.png", 0x6f64148898d2830a },
		{ "428.png", 0xef3dd9f655b37c41 },
		{ "429.png", 0x5d72ddd03fb70c4f },
		{ "430.png", 0xff8b9463b7bdb1a },
		{ "431.png", 0xfa57a304f5a74df5 },
		{ "432.png", 0xb94976a32e131cc6 },
		{ "433.png", 0x4366e4e7fad4fcfb },
		{ "434.png", 0xcac527e9ccfa4421 },
		{ "435.png", 0x2cbf811e5e4f2f88 },
		{ "436.png", 0xdb7c3963310e9096 },
		{ "437.png", 0x7b28ff76bf0f9624 },
		{ "438.png", 0x5347ed3bcb793e69 },
		{ "439.png", 0xc7082ed728beb5a0 },
		{ "440.png", 0x7075df723f4b0dbe },
		{ "441.png", 0xf1beb81f93b92263 },
		{ "442.png", 0x9766541356700116 },
		{ "443.png", 0x8983cdb18b5f7541 },
		{ "444.png", 0x489d8dbaf2e87e72 },
		{ "445.png", 0x15405fc824ba913b },
		{ "446.png", 0xb10f267e6750460e },
		{ "447.png", 0xc8822dd2cf9e46f4 },
		{ "448.png", 0x10de34a161d9fd6c },
		{ "449.png", 0xed2adae6920a701 },
		{ "450.png", 0x579d953ade544982 },
		{ "451.png", 0x3597ccdca640ffa },
		{ "452.png", 0x3dd6238623ae5a71 },
		{ "453.png", 0xc063bbef952e71fc },
		{ "454.png", 0x5d9973b9b6e35a1d },
		{ "455.png", 0x536e38fd50a3c840 },
		{ "456.png", 0x87856a069a075c46 },
		{ "457.png", 0xc27a97de39d9123 },
		{ "458.png", 0x820c5199d1aaa3c8 },
		{ "459.png", 0x7e6bfb055e340efe },
		{ "460.png", 0x9a5ca715e43a8daf },
		{ "461.png", 0xb2ba0187c4f1933d },
		{ "462.png", 0xfa6660138f76e5d0 },
		{ "463.png", 0xc1882e8e8584a60c },
		{ "464.png", 0x6f7376ff39d0ff5f },
		{ "465.png", 0x600404e1d639f51f },
		{ "466.png", 0xbb211e9ba6b34e00 },
		{ "467.png", 0x4a8d9241b4e8567c },
		{ "468.png", 0xf15a69f9d80b7b83 },
		{ "469.png", 0x3c0d16039d4aa385 },
		{ "470.png", 0x90a16650b7c6efe0 },
		{ "471.png", 0x1e17c5d0a956febf },
		{ "472.png", 0x45b88f30359ba61f },
		{ "473.png", 0xcce8f1fc8e3561fc },
		{ "474.png", 0xfae973284255522a },
		{ "475.png", 0xf4b8ad572bd5874b },
		{ "476.png", 0xa452d45141895845 },
		{ "477.png", 0x7d6f455f0aba0f8 },
		{ "478.png", 0xf03bc464059e81b7 },
		{ "479.png", 0xcc4eef3a2751a4b },
		{ "480.png", 0x10fa35dca0dfcdef },
		{ "481.png", 0xa0543fb4e210658b },
		{ "482.png", 0xe74024ddc7da2b1b },
		{ "483.png", 0x5a98a6cb654a3ab0 },
		{ "484.png", 0x8f49f64376021be3 },
		{ "485.png", 0x19e623928f9eca3a },
		{ "486.png", 0x2d0766afb1659a18 },
		{ "487.png", 0x41e4d2762dd992fa },
		{ "488.png", 0xa653f0df068324b3 },
		{ "489.png", 0x37ec01b505c70263 },
		{ "490.png", 0x6c16333de3a7826b },
		{ "491.png", 0x7ad985e690aeb481 },
		{ "492.png", 0x8f3384aa7bf61474 },
		{ "493.png", 0xddfa433d6bd8759d },
		{ "494.png", 0xd7c97d0c5ef63178 },
		{ "495.png", 0x1639a850f819d108 },
		{ "496.png", 0x7cb332cec8431c88 },
		{ "497.png", 0x3c921aa0f7d4430f },
		{ "498.png", 0x55485d504b07b274 },
		{ "499.png", 0x9ca22294ccb5c9ad },
		{ "500.png", 0xd77934778271d86f },
		{ "501.png", 0xfd04c8b55aec641e },
		{ "502.png", 0xcad7d19fba69e719 },
		{ "503.png", 0xe0cc5d7a603a34d5 },
		{ "504.png", 0x267074a2ecc725a1 },
		{ "505.png", 0x91abc251dcae3724 },
		{ "506.png", 0x6e3c0c5e3f29b09b },
		{ "507.png", 0x96cc36b6adb001a5 },
		{ "508.png", 0x977011ec7edcc1a0 },
		{ "509.png", 0x62d2ad359a512f8f },
		{ "510.png", 0xe7b3c5482b4286a },
		{ "511.png", 0x9c494e96369cdf55 },
		{ "512.png", 0x6668c0ebedb2edcf },
		{ "513.png", 0xd5afb7de24bdcdb7 },
		{ "514.png", 0x169d7a0ac570b93c },
		{ "515.png", 0x26ae56d37ff1ad8b },
		{ "516.png", 0x57743af88beaa90f },
		{ "517.png", 0xaac8c4f8b7a686de },
		{ "518.png", 0x3c6b0681ed33610a },
		{ "519.png", 0x501c6f8a4db22d7d },
		{ "520.png", 0x53242cb7f64d2a00 },
		{ "521.png", 0x93603ddd09d90593 },
		{ "522.png", 0x6b21bf834b108c19 },
		{ "523.png", 0xc334dbb8d158c895 },
		{ "524.png", 0x75040818dde4783a },
		{ "525.png", 0x1900317d4c5ab9e4 },
		{ "526.png", 0x1c7b69d6557fdbf },
		{ "527.png", 0x6d850a62ba7a3d8d },
		{ "528.png", 0x1df987ae0b8173ce },
		{ "529.png", 0xdf184ba46b34dd47 },
		{ "530.png", 0x4a1eb4827e954a62 },
		{ "531.png", 0xef25a290b03854a3 },
		{ "532.png", 0x5ab182e69f1cc5a9 },
		{ "533.png", 0xbdfe93005f906da8 },
		{ "534.png", 0x17568df0f2519fbc },
		{ "535.png", 0x329997da3ee52122 },
		{ "536.png", 0x55c8caab845b799b },
		{ "537.png", 0xec389bc176274b46 },
		{ "538.png", 0x8047710025c9293e },
		{ "539.png", 0x45c3e7d4d6b00475 },
		{ "palette000.pal", 0x565fb1599dd869fe },
		{ "palette001.pal", 0x315f8a0040cbda56 }
	}.ToFrozenDictionary();

	private const string TEST_PATH = "test-data\\th075";
	private const string ENTRIES_PATH = $"{TEST_PATH}\\entries-png";

	public PngArchiveTests() => Directory.CreateDirectory(ENTRIES_PATH);

	[Theory]
	[InlineData($"{TEST_PATH}\\07.dat")]
	public void ReadPngArchive(string path)
	{
		Guard.FailIfFileDoesNotExist(path);

		using PngArchive archive = PngArchive.Read(new FileStream(path, FileUtils.OpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.True(archive.Entries.Count() >= s_entryHashes.Count);

		ReadOnlyMemory<byte> paletteData = archive.Extract(archive.Entries.First(entry => entry.FileName.StartsWith("palette", StringComparison.OrdinalIgnoreCase)), []).ToArray();

		Assert.All(archive.Entries, entry =>
		{
			Assert.True(entry.Offset > 0);
			Assert.True(entry.Size is > 0 or -1);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlySpan<byte> entryData = archive.Extract(entry, paletteData.Span);

			Assert.False(entryData.IsEmpty);
			Assert.True(entryData.Length >= entry.Size);
			Assert.StrictEqual(s_entryHashes[entry.FileName], XxHash3.HashToUInt64(entryData));
		});
	}

	[Theory]
	[InlineData($"{TEST_PATH}\\07.dat", true)]
	public async Task ReadPngArchiveAsync(string path, bool writeEntriesToDisk)
	{
		Guard.FailIfFileDoesNotExist(path);

		await using PngArchive archive = await PngArchive.ReadAsync(new FileStream(path, FileUtils.AsyncOpenReadFileStreamOptions));

		Assert.NotEmpty(archive.Entries);
		Assert.Distinct(archive.Entries);
		Assert.True(archive.Entries.Count() >= s_entryHashes.Count);

		ReadOnlyMemory<byte> paletteData = await archive.ExtractAsync(archive.Entries.First(entry => entry.FileName.StartsWith("palette", StringComparison.OrdinalIgnoreCase)), ReadOnlyMemory<byte>.Empty);

		await Assert.AllAsync(archive.Entries, async entry =>
		{
			Assert.True(entry.Offset > 0);
			Assert.True(entry.Size is > 0 or -1);
			Assert.Contains(entry.FileName, (IReadOnlyDictionary<string, ulong>)s_entryHashes);

			ReadOnlyMemory<byte> entryData = await archive.ExtractAsync(entry, paletteData);

			Assert.False(entryData.IsEmpty);
			Assert.True(entryData.Length >= entry.Size);
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
}
