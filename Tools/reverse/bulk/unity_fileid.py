# Unity fileID for a MonoBehaviour class inside a managed DLL: first 4 bytes (LE int32) of MD4("s\0\0\0" + namespace + name)
import struct, sys
def md4(msg):
    def F(x,y,z): return (x&y)|(~x&z)
    def G(x,y,z): return (x&y)|(x&z)|(y&z)
    def H(x,y,z): return x^y^z
    rol=lambda x,n:((x<<n)|(x>>(32-n)))&0xffffffff
    ml=len(msg)*8; msg+=b'\x80'; msg+=b'\x00'*((56-len(msg)%64)%64); msg+=struct.pack('<Q',ml)
    a,b,c,d=0x67452301,0xefcdab89,0x98badcfe,0x10325476
    for i in range(0,len(msg),64):
        X=struct.unpack('<16I',msg[i:i+64]); A,B,C,D=a,b,c,d
        for k,s in zip(range(16),[3,7,11,19]*4):
            a=rol((a+F(b,c,d)+X[k])&0xffffffff,s); a,b,c,d=d,a,b,c
        for k,s in zip([0,4,8,12,1,5,9,13,2,6,10,14,3,7,11,15],[3,5,9,13]*4):
            a=rol((a+G(b,c,d)+X[k]+0x5a827999)&0xffffffff,s); a,b,c,d=d,a,b,c
        for k,s in zip([0,8,4,12,2,10,6,14,1,9,5,13,3,11,7,15],[3,9,11,15]*4):
            a=rol((a+H(b,c,d)+X[k]+0x6ed9eba1)&0xffffffff,s); a,b,c,d=d,a,b,c
        a=(a+A)&0xffffffff;b=(b+B)&0xffffffff;c=(c+C)&0xffffffff;d=(d+D)&0xffffffff
    return struct.pack('<4I',a,b,c,d)
def fileid(ns,name):
    return struct.unpack('<i',md4(b's\x00\x00\x00'+(ns+name).encode())[:4])[0]
if __name__=='__main__':
    # self-test: known value for UnityEngine.UI.Image in UnityEngine.UI.dll = -765806418
    assert fileid('UnityEngine.UI','Image')==-765806418, fileid('UnityEngine.UI','Image')
    print(fileid(sys.argv[1],sys.argv[2]))
