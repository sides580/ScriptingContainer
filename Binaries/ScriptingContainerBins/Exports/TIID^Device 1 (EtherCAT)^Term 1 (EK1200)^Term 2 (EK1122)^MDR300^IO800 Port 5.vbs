'vendorid: 26, deviceid: 6488065, revision: 1.0
Dim NetId
Dim IoLinkPort

if WScript.Arguments.Count = 2 then
  NetId = WScript.Arguments(0)
  IoLinkPort = WScript.Arguments(1) - 1 + 4096
else
  NetId = "192.168.1.3.2.8"
  IoLinkPort = 4100
end if

Dim TcClientSync
Set TcClientSync = CreateObject("TCSCRIPT.TcScriptSync")
Call TcClientSync.ConnectTo( NetId, IoLinkPort )

dim data()


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65537, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65538, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65539, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65540, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65541, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65542, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65543, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65544, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65545, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65546, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65547, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65548, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65549, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65550, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65551, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 65552, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 131072, data)


redim data(63)
data(0) = 0
data(1) = 0
data(2) = 0
data(3) = 0
data(4) = 0
data(5) = 0
data(6) = 0
data(7) = 0
data(8) = 0
data(9) = 0
data(10) = 0
data(11) = 0
data(12) = 0
data(13) = 0
data(14) = 0
data(15) = 0
data(16) = 0
data(17) = 0
data(18) = 0
data(19) = 0
data(20) = 0
data(21) = 0
data(22) = 0
data(23) = 0
data(24) = 0
data(25) = 0
data(26) = 0
data(27) = 0
data(28) = 0
data(29) = 0
data(30) = 0
data(31) = 0
data(32) = 0
data(33) = 0
data(34) = 0
data(35) = 0
data(36) = 0
data(37) = 0
data(38) = 0
data(39) = 0
data(40) = 0
data(41) = 0
data(42) = 0
data(43) = 0
data(44) = 0
data(45) = 0
data(46) = 0
data(47) = 0
data(48) = 0
data(49) = 0
data(50) = 0
data(51) = 0
data(52) = 0
data(53) = 0
data(54) = 0
data(55) = 0
data(56) = 0
data(57) = 0
data(58) = 0
data(59) = 0
data(60) = 0
data(61) = 0
data(62) = 0
data(63) = 0
call TcClientSync.WriteArrayOfInt8(62210, 1572864, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 4194304, data)


redim data(0)
data(0) = 1
call TcClientSync.WriteArrayOfInt8(62210, 4259840, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 4325376, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 4390912, data)


redim data(0)
data(0) = 1
call TcClientSync.WriteArrayOfInt8(62210, 4456448, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 4521984, data)


redim data(1)
data(0) = 0
data(1) = 50
call TcClientSync.WriteArrayOfInt8(62210, 4587520, data)


redim data(1)
data(0) = 0
data(1) = 25
call TcClientSync.WriteArrayOfInt8(62210, 4653056, data)


redim data(1)
data(0) = 39
data(1) = 16
call TcClientSync.WriteArrayOfInt8(62210, 4718592, data)


redim data(1)
data(0) = 0
data(1) = 25
call TcClientSync.WriteArrayOfInt8(62210, 4784128, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 4849664, data)


redim data(1)
data(0) = 0
data(1) = 0
call TcClientSync.WriteArrayOfInt8(62210, 4915200, data)


redim data(1)
data(0) = 0
data(1) = 25
call TcClientSync.WriteArrayOfInt8(62210, 4980736, data)


redim data(1)
data(0) = 0
data(1) = 0
call TcClientSync.WriteArrayOfInt8(62210, 5046272, data)


redim data(1)
data(0) = 0
data(1) = 25
call TcClientSync.WriteArrayOfInt8(62210, 5111808, data)


redim data(1)
data(0) = 0
data(1) = 55
call TcClientSync.WriteArrayOfInt8(62210, 5177344, data)


redim data(1)
data(0) = 0
data(1) = 25
call TcClientSync.WriteArrayOfInt8(62210, 5242880, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 5308416, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 5373952, data)


redim data(0)
data(0) = 3
call TcClientSync.WriteArrayOfInt8(62210, 5439488, data)


redim data(3)
data(0) = 0
data(1) = 0
data(2) = 0
data(3) = 0
call TcClientSync.WriteArrayOfInt8(62210, 5505024, data)


redim data(1)
data(0) = 0
data(1) = 0
call TcClientSync.WriteArrayOfInt8(62210, 5570560, data)


redim data(15)
data(0) = 0
data(1) = 0
data(2) = 0
data(3) = 0
data(4) = 0
data(5) = 0
data(6) = 0
data(7) = 0
data(8) = 0
data(9) = 0
data(10) = 0
data(11) = 0
data(12) = 0
data(13) = 0
data(14) = 0
data(15) = 0
call TcClientSync.WriteArrayOfInt8(62210, 5963776, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 6029312, data)


redim data(1)
data(0) = 0
data(1) = 0
call TcClientSync.WriteArrayOfInt8(62210, 6094848, data)


redim data(1)
data(0) = 0
data(1) = 0
call TcClientSync.WriteArrayOfInt8(62210, 6160384, data)


redim data(1)
data(0) = 0
data(1) = 0
call TcClientSync.WriteArrayOfInt8(62210, 6225920, data)


redim data(1)
data(0) = 0
data(1) = 99
call TcClientSync.WriteArrayOfInt8(62210, 6291456, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 6356992, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 6422528, data)


redim data(0)
data(0) = 1
call TcClientSync.WriteArrayOfInt8(62210, 6488064, data)


redim data(0)
data(0) = 2
call TcClientSync.WriteArrayOfInt8(62210, 6750208, data)


redim data(0)
data(0) = 0
call TcClientSync.WriteArrayOfInt8(62210, 6815744, data)


redim data(0)
data(0) = 1
call TcClientSync.WriteArrayOfInt8(62210, 6881280, data)


redim data(1)
data(0) = 0
data(1) = 0
call TcClientSync.WriteArrayOfInt8(62210, 6946816, data)


redim data(1)
data(0) = 0
data(1) = 0
call TcClientSync.WriteArrayOfInt8(62210, 7012352, data)


redim data(1)
data(0) = 0
data(1) = 0
call TcClientSync.WriteArrayOfInt8(62210, 8519680, data)


