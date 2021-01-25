using System;

namespace HashLib.Crypto
{
    internal class RIPEMD320 : MDBase
    {
        public RIPEMD320() : base(10, 40)
        {
        }

        public override void Initialize()
        {
            m_state[4] = 0xc3d2e1f0;
            m_state[5] = 0x76543210;
            m_state[6] = 0xFEDCBA98;
            m_state[7] = 0x89ABCDEF;
            m_state[8] = 0x01234567;
            m_state[9] = 0x3C2D1E0F;

            base.Initialize();
        }

        protected override void TransformBlock(Span<byte> a_data, int a_index)
        {
            uint data0 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 0);
            uint data1 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 1);
            uint data2 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 2);
            uint data3 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 3);
            uint data4 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 4);
            uint data5 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 5);
            uint data6 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 6);
            uint data7 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 7);
            uint data8 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 8);
            uint data9 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 9);
            uint data10 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 10);
            uint data11 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 11);
            uint data12 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 12);
            uint data13 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 13);
            uint data14 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 14);
            uint data15 = Converters.ConvertBytesToUInt(a_data, a_index + 4 * 15);

            uint a = m_state[0];
            uint b = m_state[1];
            uint c = m_state[2];
            uint d = m_state[3];
            uint e = m_state[4];
            uint aa = m_state[5];
            uint bb = m_state[6];
            uint cc = m_state[7];
            uint dd = m_state[8];
            uint ee = m_state[9];

            a += data0 + (b ^ c ^ d);
            a = ((a << 11) | (a >> (32 - 11))) + e;
            c = (c << 10) | (c >> (32 - 10));
            e += data1 + (a ^ b ^ c);
            e = ((e << 14) | (e >> (32 - 14))) + d;
            b = (b << 10) | (b >> (32 - 10));
            d += data2 + (e ^ a ^ b);
            d = ((d << 15) | (d >> (32 - 15))) + c;
            a = (a << 10) | (a >> (32 - 10));
            c += data3 + (d ^ e ^ a);
            c = ((c << 12) | (c >> (32 - 12))) + b;
            e = (e << 10) | (e >> (32 - 10));
            b += data4 + (c ^ d ^ e);
            b = ((b << 5) | (b >> (32 - 5))) + a;
            d = (d << 10) | (d >> (32 - 10));
            a += data5 + (b ^ c ^ d);
            a = ((a << 8) | (a >> (32 - 8))) + e;
            c = (c << 10) | (c >> (32 - 10));
            e += data6 + (a ^ b ^ c);
            e = ((e << 7) | (e >> (32 - 7))) + d;
            b = (b << 10) | (b >> (32 - 10));
            d += data7 + (e ^ a ^ b);
            d = ((d << 9) | (d >> (32 - 9))) + c;
            a = (a << 10) | (a >> (32 - 10));
            c += data8 + (d ^ e ^ a);
            c = ((c << 11) | (c >> (32 - 11))) + b;
            e = (e << 10) | (e >> (32 - 10));
            b += data9 + (c ^ d ^ e);
            b = ((b << 13) | (b >> (32 - 13))) + a;
            d = (d << 10) | (d >> (32 - 10));
            a += data10 + (b ^ c ^ d);
            a = ((a << 14) | (a >> (32 - 14))) + e;
            c = (c << 10) | (c >> (32 - 10));
            e += data11 + (a ^ b ^ c);
            e = ((e << 15) | (e >> (32 - 15))) + d;
            b = (b << 10) | (b >> (32 - 10));
            d += data12 + (e ^ a ^ b);
            d = ((d << 6) | (d >> (32 - 6))) + c;
            a = (a << 10) | (a >> (32 - 10));
            c += data13 + (d ^ e ^ a);
            c = ((c << 7) | (c >> (32 - 7))) + b;
            e = (e << 10) | (e >> (32 - 10));
            b += data14 + (c ^ d ^ e);
            b = ((b << 9) | (b >> (32 - 9))) + a;
            d = (d << 10) | (d >> (32 - 10));
            a += data15 + (b ^ c ^ d);
            a = ((a << 8) | (a >> (32 - 8))) + e;
            c = (c << 10) | (c >> (32 - 10));

            aa += data5 + C1 + (bb ^ (cc | ~dd));
            aa = ((aa << 8) | (aa >> (32 - 8))) + ee;
            cc = (cc << 10) | (cc >> (32 - 10));
            ee += data14 + C1 + (aa ^ (bb | ~cc));
            ee = ((ee << 9) | (ee >> (32 - 9))) + dd;
            bb = (bb << 10) | (bb >> (32 - 10));
            dd += data7 + C1 + (ee ^ (aa | ~bb));
            dd = ((dd << 9) | (dd >> (32 - 9))) + cc;
            aa = (aa << 10) | (aa >> (32 - 10));
            cc += data0 + C1 + (dd ^ (ee | ~aa));
            cc = ((cc << 11) | (cc >> (32 - 11))) + bb;
            ee = (ee << 10) | (ee >> (32 - 10));
            bb += data9 + C1 + (cc ^ (dd | ~ee));
            bb = ((bb << 13) | (bb >> (32 - 13))) + aa;
            dd = (dd << 10) | (dd >> (32 - 10));
            aa += data2 + C1 + (bb ^ (cc | ~dd));
            aa = ((aa << 15) | (aa >> (32 - 15))) + ee;
            cc = (cc << 10) | (cc >> (32 - 10));
            ee += data11 + C1 + (aa ^ (bb | ~cc));
            ee = ((ee << 15) | (ee >> (32 - 15))) + dd;
            bb = (bb << 10) | (bb >> (32 - 10));
            dd += data4 + C1 + (ee ^ (aa | ~bb));
            dd = ((dd << 5) | (dd >> (32 - 5))) + cc;
            aa = (aa << 10) | (aa >> (32 - 10));
            cc += data13 + C1 + (dd ^ (ee | ~aa));
            cc = ((cc << 7) | (cc >> (32 - 7))) + bb;
            ee = (ee << 10) | (ee >> (32 - 10));
            bb += data6 + C1 + (cc ^ (dd | ~ee));
            bb = ((bb << 7) | (bb >> (32 - 7))) + aa;
            dd = (dd << 10) | (dd >> (32 - 10));
            aa += data15 + C1 + (bb ^ (cc | ~dd));
            aa = ((aa << 8) | (aa >> (32 - 8))) + ee;
            cc = (cc << 10) | (cc >> (32 - 10));
            ee += data8 + C1 + (aa ^ (bb | ~cc));
            ee = ((ee << 11) | (ee >> (32 - 11))) + dd;
            bb = (bb << 10) | (bb >> (32 - 10));
            dd += data1 + C1 + (ee ^ (aa | ~bb));
            dd = ((dd << 14) | (dd >> (32 - 14))) + cc;
            aa = (aa << 10) | (aa >> (32 - 10));
            cc += data10 + C1 + (dd ^ (ee | ~aa));
            cc = ((cc << 14) | (cc >> (32 - 14))) + bb;
            ee = (ee << 10) | (ee >> (32 - 10));
            bb += data3 + C1 + (cc ^ (dd | ~ee));
            bb = ((bb << 12) | (bb >> (32 - 12))) + aa;
            dd = (dd << 10) | (dd >> (32 - 10));
            aa += data12 + C1 + (bb ^ (cc | ~dd));
            aa = ((aa << 6) | (aa >> (32 - 6))) + ee;
            cc = (cc << 10) | (cc >> (32 - 10));

            e += data7 + C2 + ((aa & b) | (~aa & c));
            e = ((e << 7) | (e >> (32 - 7))) + d;
            b = (b << 10) | (b >> (32 - 10));
            d += data4 + C2 + ((e & aa) | (~e & b));
            d = ((d << 6) | (d >> (32 - 6))) + c;
            aa = (aa << 10) | (aa >> (32 - 10));
            c += data13 + C2 + ((d & e) | (~d & aa));
            c = ((c << 8) | (c >> (32 - 8))) + b;
            e = (e << 10) | (e >> (32 - 10));
            b += data1 + C2 + ((c & d) | (~c & e));
            b = ((b << 13) | (b >> (32 - 13))) + aa;
            d = (d << 10) | (d >> (32 - 10));
            aa += data10 + C2 + ((b & c) | (~b & d));
            aa = ((aa << 11) | (aa >> (32 - 11))) + e;
            c = (c << 10) | (c >> (32 - 10));
            e += data6 + C2 + ((aa & b) | (~aa & c));
            e = ((e << 9) | (e >> (32 - 9))) + d;
            b = (b << 10) | (b >> (32 - 10));
            d += data15 + C2 + ((e & aa) | (~e & b));
            d = ((d << 7) | (d >> (32 - 7))) + c;
            aa = (aa << 10) | (aa >> (32 - 10));
            c += data3 + C2 + ((d & e) | (~d & aa));
            c = ((c << 15) | (c >> (32 - 15))) + b;
            e = (e << 10) | (e >> (32 - 10));
            b += data12 + C2 + ((c & d) | (~c & e));
            b = ((b << 7) | (b >> (32 - 7))) + aa;
            d = (d << 10) | (d >> (32 - 10));
            aa += data0 + C2 + ((b & c) | (~b & d));
            aa = ((aa << 12) | (aa >> (32 - 12))) + e;
            c = (c << 10) | (c >> (32 - 10));
            e += data9 + C2 + ((aa & b) | (~aa & c));
            e = ((e << 15) | (e >> (32 - 15))) + d;
            b = (b << 10) | (b >> (32 - 10));
            d += data5 + C2 + ((e & aa) | (~e & b));
            d = ((d << 9) | (d >> (32 - 9))) + c;
            aa = (aa << 10) | (aa >> (32 - 10));
            c += data2 + C2 + ((d & e) | (~d & aa));
            c = ((c << 11) | (c >> (32 - 11))) + b;
            e = (e << 10) | (e >> (32 - 10));
            b += data14 + C2 + ((c & d) | (~c & e));
            b = ((b << 7) | (b >> (32 - 7))) + aa;
            d = (d << 10) | (d >> (32 - 10));
            aa += data11 + C2 + ((b & c) | (~b & d));
            aa = ((aa << 13) | (aa >> (32 - 13))) + e;
            c = (c << 10) | (c >> (32 - 10));
            e += data8 + C2 + ((aa & b) | (~aa & c));
            e = ((e << 12) | (e >> (32 - 12))) + d;
            b = (b << 10) | (b >> (32 - 10));

            ee += data6 + C3 + ((a & cc) | (bb & ~cc));
            ee = ((ee << 9) | (ee >> (32 - 9))) + dd;
            bb = (bb << 10) | (bb >> (32 - 10));
            dd += data11 + C3 + ((ee & bb) | (a & ~bb));
            dd = ((dd << 13) | (dd >> (32 - 13))) + cc;
            a = (a << 10) | (a >> (32 - 10));
            cc += data3 + C3 + ((dd & a) | (ee & ~a));
            cc = ((cc << 15) | (cc >> (32 - 15))) + bb;
            ee = (ee << 10) | (ee >> (32 - 10));
            bb += data7 + C3 + ((cc & ee) | (dd & ~ee));
            bb = ((bb << 7) | (bb >> (32 - 7))) + a;
            dd = (dd << 10) | (dd >> (32 - 10));
            a += data0 + C3 + ((bb & dd) | (cc & ~dd));
            a = ((a << 12) | (a >> (32 - 12))) + ee;
            cc = (cc << 10) | (cc >> (32 - 10));
            ee += data13 + C3 + ((a & cc) | (bb & ~cc));
            ee = ((ee << 8) | (ee >> (32 - 8))) + dd;
            bb = (bb << 10) | (bb >> (32 - 10));
            dd += data5 + C3 + ((ee & bb) | (a & ~bb));
            dd = ((dd << 9) | (dd >> (32 - 9))) + cc;
            a = (a << 10) | (a >> (32 - 10));
            cc += data10 + C3 + ((dd & a) | (ee & ~a));
            cc = ((cc << 11) | (cc >> (32 - 11))) + bb;
            ee = (ee << 10) | (ee >> (32 - 10));
            bb += data14 + C3 + ((cc & ee) | (dd & ~ee));
            bb = ((bb << 7) | (bb >> (32 - 7))) + a;
            dd = (dd << 10) | (dd >> (32 - 10));
            a += data15 + C3 + ((bb & dd) | (cc & ~dd));
            a = ((a << 7) | (a >> (32 - 7))) + ee;
            cc = (cc << 10) | (cc >> (32 - 10));
            ee += data8 + C3 + ((a & cc) | (bb & ~cc));
            ee = ((ee << 12) | (ee >> (32 - 12))) + dd;
            bb = (bb << 10) | (bb >> (32 - 10));
            dd += data12 + C3 + ((ee & bb) | (a & ~bb));
            dd = ((dd << 7) | (dd >> (32 - 7))) + cc;
            a = (a << 10) | (a >> (32 - 10));
            cc += data4 + C3 + ((dd & a) | (ee & ~a));
            cc = ((cc << 6) | (cc >> (32 - 6))) + bb;
            ee = (ee << 10) | (ee >> (32 - 10));
            bb += data9 + C3 + ((cc & ee) | (dd & ~ee));
            bb = ((bb << 15) | (bb >> (32 - 15))) + a;
            dd = (dd << 10) | (dd >> (32 - 10));
            a += data1 + C3 + ((bb & dd) | (cc & ~dd));
            a = ((a << 13) | (a >> (32 - 13))) + ee;
            cc = (cc << 10) | (cc >> (32 - 10));
            ee += data2 + C3 + ((a & cc) | (bb & ~cc));
            ee = ((ee << 11) | (ee >> (32 - 11))) + dd;
            bb = (bb << 10) | (bb >> (32 - 10));

            d += data3 + C4 + ((e | ~aa) ^ bb);
            d = ((d << 11) | (d >> (32 - 11))) + c;
            aa = (aa << 10) | (aa >> (32 - 10));
            c += data10 + C4 + ((d | ~e) ^ aa);
            c = ((c << 13) | (c >> (32 - 13))) + bb;
            e = (e << 10) | (e >> (32 - 10));
            bb += data14 + C4 + ((c | ~d) ^ e);
            bb = ((bb << 6) | (bb >> (32 - 6))) + aa;
            d = (d << 10) | (d >> (32 - 10));
            aa += data4 + C4 + ((bb | ~c) ^ d);
            aa = ((aa << 7) | (aa >> (32 - 7))) + e;
            c = (c << 10) | (c >> (32 - 10));
            e += data9 + C4 + ((aa | ~bb) ^ c);
            e = ((e << 14) | (e >> (32 - 14))) + d;
            bb = (bb << 10) | (bb >> (32 - 10));
            d += data15 + C4 + ((e | ~aa) ^ bb);
            d = ((d << 9) | (d >> (32 - 9))) + c;
            aa = (aa << 10) | (aa >> (32 - 10));
            c += data8 + C4 + ((d | ~e) ^ aa);
            c = ((c << 13) | (c >> (32 - 13))) + bb;
            e = (e << 10) | (e >> (32 - 10));
            bb += data1 + C4 + ((c | ~d) ^ e);
            bb = ((bb << 15) | (bb >> (32 - 15))) + aa;
            d = (d << 10) | (d >> (32 - 10));
            aa += data2 + C4 + ((bb | ~c) ^ d);
            aa = ((aa << 14) | (aa >> (32 - 14))) + e;
            c = (c << 10) | (c >> (32 - 10));
            e += data7 + C4 + ((aa | ~bb) ^ c);
            e = ((e << 8) | (e >> (32 - 8))) + d;
            bb = (bb << 10) | (bb >> (32 - 10));
            d += data0 + C4 + ((e | ~aa) ^ bb);
            d = ((d << 13) | (d >> (32 - 13))) + c;
            aa = (aa << 10) | (aa >> (32 - 10));
            c += data6 + C4 + ((d | ~e) ^ aa);
            c = ((c << 6) | (c >> (32 - 6))) + bb;
            e = (e << 10) | (e >> (32 - 10));
            bb += data13 + C4 + ((c | ~d) ^ e);
            bb = ((bb << 5) | (bb >> (32 - 5))) + aa;
            d = (d << 10) | (d >> (32 - 10));
            aa += data11 + C4 + ((bb | ~c) ^ d);
            aa = ((aa << 12) | (aa >> (32 - 12))) + e;
            c = (c << 10) | (c >> (32 - 10));
            e += data5 + C4 + ((aa | ~bb) ^ c);
            e = ((e << 7) | (e >> (32 - 7))) + d;
            bb = (bb << 10) | (bb >> (32 - 10));
            d += data12 + C4 + ((e | ~aa) ^ bb);
            d = ((d << 5) | (d >> (32 - 5))) + c;
            aa = (aa << 10) | (aa >> (32 - 10));

            dd += data15 + C5 + ((ee | ~a) ^ b);
            dd = ((dd << 9) | (dd >> (32 - 9))) + cc;
            a = (a << 10) | (a >> (32 - 10));
            cc += data5 + C5 + ((dd | ~ee) ^ a);
            cc = ((cc << 7) | (cc >> (32 - 7))) + b;
            ee = (ee << 10) | (ee >> (32 - 10));
            b += data1 + C5 + ((cc | ~dd) ^ ee);
            b = ((b << 15) | (b >> (32 - 15))) + a;
            dd = (dd << 10) | (dd >> (32 - 10));
            a += data3 + C5 + ((b | ~cc) ^ dd);
            a = ((a << 11) | (a >> (32 - 11))) + ee;
            cc = (cc << 10) | (cc >> (32 - 10));
            ee += data7 + C5 + ((a | ~b) ^ cc);
            ee = ((ee << 8) | (ee >> (32 - 8))) + dd;
            b = (b << 10) | (b >> (32 - 10));
            dd += data14 + C5 + ((ee | ~a) ^ b);
            dd = ((dd << 6) | (dd >> (32 - 6))) + cc;
            a = (a << 10) | (a >> (32 - 10));
            cc += data6 + C5 + ((dd | ~ee) ^ a);
            cc = ((cc << 6) | (cc >> (32 - 6))) + b;
            ee = (ee << 10) | (ee >> (32 - 10));
            b += data9 + C5 + ((cc | ~dd) ^ ee);
            b = ((b << 14) | (b >> (32 - 14))) + a;
            dd = (dd << 10) | (dd >> (32 - 10));
            a += data11 + C5 + ((b | ~cc) ^ dd);
            a = ((a << 12) | (a >> (32 - 12))) + ee;
            cc = (cc << 10) | (cc >> (32 - 10));
            ee += data8 + C5 + ((a | ~b) ^ cc);
            ee = ((ee << 13) | (ee >> (32 - 13))) + dd;
            b = (b << 10) | (b >> (32 - 10));
            dd += data12 + C5 + ((ee | ~a) ^ b);
            dd = ((dd << 5) | (dd >> (32 - 5))) + cc;
            a = (a << 10) | (a >> (32 - 10));
            cc += data2 + C5 + ((dd | ~ee) ^ a);
            cc = ((cc << 14) | (cc >> (32 - 14))) + b;
            ee = (ee << 10) | (ee >> (32 - 10));
            b += data10 + C5 + ((cc | ~dd) ^ ee);
            b = ((b << 13) | (b >> (32 - 13))) + a;
            dd = (dd << 10) | (dd >> (32 - 10));
            a += data0 + C5 + ((b | ~cc) ^ dd);
            a = ((a << 13) | (a >> (32 - 13))) + ee;
            cc = (cc << 10) | (cc >> (32 - 10));
            ee += data4 + C5 + ((a | ~b) ^ cc);
            ee = ((ee << 7) | (ee >> (32 - 7))) + dd;
            b = (b << 10) | (b >> (32 - 10));
            dd += data13 + C5 + ((ee | ~a) ^ b);
            dd = ((dd << 5) | (dd >> (32 - 5))) + cc;
            a = (a << 10) | (a >> (32 - 10));

            cc += data1 + C6 + ((d & aa) | (e & ~aa));
            cc = ((cc << 11) | (cc >> (32 - 11))) + bb;
            e = (e << 10) | (e >> (32 - 10));
            bb += data9 + C6 + ((cc & e) | (d & ~e));
            bb = ((bb << 12) | (bb >> (32 - 12))) + aa;
            d = (d << 10) | (d >> (32 - 10));
            aa += data11 + C6 + ((bb & d) | (cc & ~d));
            aa = ((aa << 14) | (aa >> (32 - 14))) + e;
            cc = (cc << 10) | (cc >> (32 - 10));
            e += data10 + C6 + ((aa & cc) | (bb & ~cc));
            e = ((e << 15) | (e >> (32 - 15))) + d;
            bb = (bb << 10) | (bb >> (32 - 10));
            d += data0 + C6 + ((e & bb) | (aa & ~bb));
            d = ((d << 14) | (d >> (32 - 14))) + cc;
            aa = (aa << 10) | (aa >> (32 - 10));
            cc += data8 + C6 + ((d & aa) | (e & ~aa));
            cc = ((cc << 15) | (cc >> (32 - 15))) + bb;
            e = (e << 10) | (e >> (32 - 10));
            bb += data12 + C6 + ((cc & e) | (d & ~e));
            bb = ((bb << 9) | (bb >> (32 - 9))) + aa;
            d = (d << 10) | (d >> (32 - 10));
            aa += data4 + C6 + ((bb & d) | (cc & ~d));
            aa = ((aa << 8) | (aa >> (32 - 8))) + e;
            cc = (cc << 10) | (cc >> (32 - 10));
            e += data13 + C6 + ((aa & cc) | (bb & ~cc));
            e = ((e << 9) | (e >> (32 - 9))) + d;
            bb = (bb << 10) | (bb >> (32 - 10));
            d += data3 + C6 + ((e & bb) | (aa & ~bb));
            d = ((d << 14) | (d >> (32 - 14))) + cc;
            aa = (aa << 10) | (aa >> (32 - 10));
            cc += data7 + C6 + ((d & aa) | (e & ~aa));
            cc = ((cc << 5) | (cc >> (32 - 5))) + bb;
            e = (e << 10) | (e >> (32 - 10));
            bb += data15 + C6 + ((cc & e) | (d & ~e));
            bb = ((bb << 6) | (bb >> (32 - 6))) + aa;
            d = (d << 10) | (d >> (32 - 10));
            aa += data14 + C6 + ((bb & d) | (cc & ~d));
            aa = ((aa << 8) | (aa >> (32 - 8))) + e;
            cc = (cc << 10) | (cc >> (32 - 10));
            e += data5 + C6 + ((aa & cc) | (bb & ~cc));
            e = ((e << 6) | (e >> (32 - 6))) + d;
            bb = (bb << 10) | (bb >> (32 - 10));
            d += data6 + C6 + ((e & bb) | (aa & ~bb));
            d = ((d << 5) | (d >> (32 - 5))) + cc;
            aa = (aa << 10) | (aa >> (32 - 10));
            cc += data2 + C6 + ((d & aa) | (e & ~aa));
            cc = ((cc << 12) | (cc >> (32 - 12))) + bb;
            e = (e << 10) | (e >> (32 - 10));

            c += data8 + C7 + ((dd & ee) | (~dd & a));
            c = ((c << 15) | (c >> (32 - 15))) + b;
            ee = (ee << 10) | (ee >> (32 - 10));
            b += data6 + C7 + ((c & dd) | (~c & ee));
            b = ((b << 5) | (b >> (32 - 5))) + a;
            dd = (dd << 10) | (dd >> (32 - 10));
            a += data4 + C7 + ((b & c) | (~b & dd));
            a = ((a << 8) | (a >> (32 - 8))) + ee;
            c = (c << 10) | (c >> (32 - 10));
            ee += data1 + C7 + ((a & b) | (~a & c));
            ee = ((ee << 11) | (ee >> (32 - 11))) + dd;
            b = (b << 10) | (b >> (32 - 10));
            dd += data3 + C7 + ((ee & a) | (~ee & b));
            dd = ((dd << 14) | (dd >> (32 - 14))) + c;
            a = (a << 10) | (a >> (32 - 10));
            c += data11 + C7 + ((dd & ee) | (~dd & a));
            c = ((c << 14) | (c >> (32 - 14))) + b;
            ee = (ee << 10) | (ee >> (32 - 10));
            b += data15 + C7 + ((c & dd) | (~c & ee));
            b = ((b << 6) | (b >> (32 - 6))) + a;
            dd = (dd << 10) | (dd >> (32 - 10));
            a += data0 + C7 + ((b & c) | (~b & dd));
            a = ((a << 14) | (a >> (32 - 14))) + ee;
            c = (c << 10) | (c >> (32 - 10));
            ee += data5 + C7 + ((a & b) | (~a & c));
            ee = ((ee << 6) | (ee >> (32 - 6))) + dd;
            b = (b << 10) | (b >> (32 - 10));
            dd += data12 + C7 + ((ee & a) | (~ee & b));
            dd = ((dd << 9) | (dd >> (32 - 9))) + c;
            a = (a << 10) | (a >> (32 - 10));
            c += data2 + C7 + ((dd & ee) | (~dd & a));
            c = ((c << 12) | (c >> (32 - 12))) + b;
            ee = (ee << 10) | (ee >> (32 - 10));
            b += data13 + C7 + ((c & dd) | (~c & ee));
            b = ((b << 9) | (b >> (32 - 9))) + a;
            dd = (dd << 10) | (dd >> (32 - 10));
            a += data9 + C7 + ((b & c) | (~b & dd));
            a = ((a << 12) | (a >> (32 - 12))) + ee;
            c = (c << 10) | (c >> (32 - 10));
            ee += data7 + C7 + ((a & b) | (~a & c));
            ee = ((ee << 5) | (ee >> (32 - 5))) + dd;
            b = (b << 10) | (b >> (32 - 10));
            dd += data10 + C7 + ((ee & a) | (~ee & b));
            dd = ((dd << 15) | (dd >> (32 - 15))) + c;
            a = (a << 10) | (a >> (32 - 10));
            c += data14 + C7 + ((dd & ee) | (~dd & a));
            c = ((c << 8) | (c >> (32 - 8))) + b;
            ee = (ee << 10) | (ee >> (32 - 10));

            bb += data4 + C8 + (cc ^ (dd | ~e));
            bb = ((bb << 9) | (bb >> (32 - 9))) + aa;
            dd = (dd << 10) | (dd >> (32 - 10));
            aa += data0 + C8 + (bb ^ (cc | ~dd));
            aa = ((aa << 15) | (aa >> (32 - 15))) + e;
            cc = (cc << 10) | (cc >> (32 - 10));
            e += data5 + C8 + (aa ^ (bb | ~cc));
            e = ((e << 5) | (e >> (32 - 5))) + dd;
            bb = (bb << 10) | (bb >> (32 - 10));
            dd += data9 + C8 + (e ^ (aa | ~bb));
            dd = ((dd << 11) | (dd >> (32 - 11))) + cc;
            aa = (aa << 10) | (aa >> (32 - 10));
            cc += data7 + C8 + (dd ^ (e | ~aa));
            cc = ((cc << 6) | (cc >> (32 - 6))) + bb;
            e = (e << 10) | (e >> (32 - 10));
            bb += data12 + C8 + (cc ^ (dd | ~e));
            bb = ((bb << 8) | (bb >> (32 - 8))) + aa;
            dd = (dd << 10) | (dd >> (32 - 10));
            aa += data2 + C8 + (bb ^ (cc | ~dd));
            aa = ((aa << 13) | (aa >> (32 - 13))) + e;
            cc = (cc << 10) | (cc >> (32 - 10));
            e += data10 + C8 + (aa ^ (bb | ~cc));
            e = ((e << 12) | (e >> (32 - 12))) + dd;
            bb = (bb << 10) | (bb >> (32 - 10));
            dd += data14 + C8 + (e ^ (aa | ~bb));
            dd = ((dd << 5) | (dd >> (32 - 5))) + cc;
            aa = (aa << 10) | (aa >> (32 - 10));
            cc += data1 + C8 + (dd ^ (e | ~aa));
            cc = ((cc << 12) | (cc >> (32 - 12))) + bb;
            e = (e << 10) | (e >> (32 - 10));
            bb += data3 + C8 + (cc ^ (dd | ~e));
            bb = ((bb << 13) | (bb >> (32 - 13))) + aa;
            dd = (dd << 10) | (dd >> (32 - 10));
            aa += data8 + C8 + (bb ^ (cc | ~dd));
            aa = ((aa << 14) | (aa >> (32 - 14))) + e;
            cc = (cc << 10) | (cc >> (32 - 10));
            e += data11 + C8 + (aa ^ (bb | ~cc));
            e = ((e << 11) | (e >> (32 - 11))) + dd;
            bb = (bb << 10) | (bb >> (32 - 10));
            dd += data6 + C8 + (e ^ (aa | ~bb));
            dd = ((dd << 8) | (dd >> (32 - 8))) + cc;
            aa = (aa << 10) | (aa >> (32 - 10));
            cc += data15 + C8 + (dd ^ (e | ~aa));
            cc = ((cc << 5) | (cc >> (32 - 5))) + bb;
            e = (e << 10) | (e >> (32 - 10));
            bb += data13 + C8 + (cc ^ (dd | ~e));
            bb = ((bb << 6) | (bb >> (32 - 6))) + aa;
            dd = (dd << 10) | (dd >> (32 - 10));

            b += data12 + (c ^ d ^ ee);
            b = ((b << 8) | (b >> (32 - 8))) + a;
            d = (d << 10) | (d >> (32 - 10));
            a += data15 + (b ^ c ^ d);
            a = ((a << 5) | (a >> (32 - 5))) + ee;
            c = (c << 10) | (c >> (32 - 10));
            ee += data10 + (a ^ b ^ c);
            ee = ((ee << 12) | (ee >> (32 - 12))) + d;
            b = (b << 10) | (b >> (32 - 10));
            d += data4 + (ee ^ a ^ b);
            d = ((d << 9) | (d >> (32 - 9))) + c;
            a = (a << 10) | (a >> (32 - 10));
            c += data1 + (d ^ ee ^ a);
            c = ((c << 12) | (c >> (32 - 12))) + b;
            ee = (ee << 10) | (ee >> (32 - 10));
            b += data5 + (c ^ d ^ ee);
            b = ((b << 5) | (b >> (32 - 5))) + a;
            d = (d << 10) | (d >> (32 - 10));
            a += data8 + (b ^ c ^ d);
            a = ((a << 14) | (a >> (32 - 14))) + ee;
            c = (c << 10) | (c >> (32 - 10));
            ee += data7 + (a ^ b ^ c);
            ee = ((ee << 6) | (ee >> (32 - 6))) + d;
            b = (b << 10) | (b >> (32 - 10));
            d += data6 + (ee ^ a ^ b);
            d = ((d << 8) | (d >> (32 - 8))) + c;
            a = (a << 10) | (a >> (32 - 10));
            c += data2 + (d ^ ee ^ a);
            c = ((c << 13) | (c >> (32 - 13))) + b;
            ee = (ee << 10) | (ee >> (32 - 10));
            b += data13 + (c ^ d ^ ee);
            b = ((b << 6) | (b >> (32 - 6))) + a;
            d = (d << 10) | (d >> (32 - 10));
            a += data14 + (b ^ c ^ d);
            a = ((a << 5) | (a >> (32 - 5))) + ee;
            c = (c << 10) | (c >> (32 - 10));
            ee += data0 + (a ^ b ^ c);
            ee = ((ee << 15) | (ee >> (32 - 15))) + d;
            b = (b << 10) | (b >> (32 - 10));
            d += data3 + (ee ^ a ^ b);
            d = ((d << 13) | (d >> (32 - 13))) + c;
            a = (a << 10) | (a >> (32 - 10));
            c += data9 + (d ^ ee ^ a);
            c = ((c << 11) | (c >> (32 - 11))) + b;
            ee = (ee << 10) | (ee >> (32 - 10));
            b += data11 + (c ^ d ^ ee);
            b = ((b << 11) | (b >> (32 - 11))) + a;
            d = (d << 10) | (d >> (32 - 10));

            m_state[0] += aa;
            m_state[1] += bb;
            m_state[2] += cc;
            m_state[3] += dd;
            m_state[4] += ee;
            m_state[5] += a;
            m_state[6] += b;
            m_state[7] += c;
            m_state[8] += d;
            m_state[9] += e;
        }
    }
}
