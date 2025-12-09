<?php

namespace App\Entity\Enum;

enum HuecoGrade: string
{
    case V_EASY = 0;
    case V0_MINUS = 1;
    case V0 = 2;
    case V0_PLUS = 3;
    case V1_MINUS = 4;
    case V1 = 5;
    case V1_PLUS = 6;
    case V2_MINUS = 7;
    case V2 = 8;
    case V2_PLUS = 9;
    case V3_MINUS = 10;
    case V3 = 11;
    case V3_PLUS = 12;
    case V4_MINUS = 13;
    case V4 = 14;
    case V4_PLUS = 15;
    case V5_MINUS = 16;
    case V5 = 17;
    case V5_PLUS = 18;
    case V6_MINUS = 19;
    case V6 = 20;
    case V6_PLUS = 21;
    case V7_MINUS = 22;
    case V7 = 23;
    case V7_PLUS = 24;
    case V8_MINUS = 25;
    case V8 = 26;
    case V8_PLUS = 27;
    case V9_MINUS = 28;
    case V9 = 29;
    case V9_PLUS = 30;
    case V10_MINUS = 31;
    case V10 = 32;
    case V10_PLUS = 33;
    case V11_MINUS = 34;
    case V11 = 35;
    case V11_PLUS = 36;
    case V12_MINUS = 37;
    case V12 = 38;
    case V12_PLUS = 39;
    case V13_MINUS = 40;
    case V13 = 41;
    case V13_PLUS = 42;
    case V14_MINUS = 43;
    case V14 = 44;
    case V14_PLUS = 45;
    case V15_MINUS = 46;
    case V15 = 47;
    case V15_PLUS = 48;
    case V16_MINUS = 49;
    case V16 = 50;
    case V16_PLUS = 51;
    case V17_MINUS = 52;
    case V17 = 53;
    case V17_PLUS = 54;
    case V18_MINUS = 55;
    case V18 = 56;
    case V18_PLUS = 57;
    case V19_MINUS = 58;
    case V19 = 59;
    case V19_PLUS = 60;

    public function GetName(): string
    {
        return match($this) {
            self::V_EASY => 'V-easy',
            self::V0_MINUS => 'V0-',
            self::V0 => 'V0',
            self::V0_PLUS => 'V0+',
            self::V1_MINUS => 'V1-',
            self::V1 => 'V1',
            self::V1_PLUS => 'V1+',
            self::V2_MINUS => 'V2-',
            self::V2 => 'V2',
            self::V2_PLUS => 'V2+',
            self::V3_MINUS => 'V3-',
            self::V3 => 'V3',
            self::V3_PLUS => 'V3+',
            self::V4_MINUS => 'V4-',
            self::V4 => 'V4',
            self::V4_PLUS => 'V4+',
            self::V5_MINUS => 'V5-',
            self::V5 => 'V5',
            self::V5_PLUS => 'V5+',
            self::V6_MINUS => 'V6-',
            self::V6 => 'V6',
            self::V6_PLUS => 'V6+',
            self::V7_MINUS => 'V7-',
            self::V7 => 'V7',
            self::V7_PLUS => 'V7+',
            self::V8_MINUS => 'V8-',
            self::V8 => 'V8',
            self::V8_PLUS => 'V8+',
            self::V9_MINUS => 'V9-',
            self::V9 => 'V9',
            self::V9_PLUS => 'V9+',
            self::V10_MINUS => 'V10-',
            self::V10 => 'V10',
            self::V10_PLUS => 'V10+',
            self::V11_MINUS => 'V11-',
            self::V11 => 'V11',
            self::V11_PLUS => 'V11+',
            self::V12_MINUS => 'V12-',
            self::V12 => 'V12',
            self::V12_PLUS => 'V12+',
            self::V13_MINUS => 'V13-',
            self::V13 => 'V13',
            self::V13_PLUS => 'V13+',
            self::V14_MINUS => 'V14-',
            self::V14 => 'V14',
            self::V14_PLUS => 'V14+',
            self::V15_MINUS => 'V15-',
            self::V15 => 'V15',
            self::V15_PLUS => 'V15+',
            self::V16_MINUS => 'V16-',
            self::V16 => 'V16',
            self::V16_PLUS => 'V16+',
            self::V17_MINUS => 'V17-',
            self::V17 => 'V17',
            self::V17_PLUS => 'V17+',
            self::V18_MINUS => 'V18-',
            self::V18 => 'V18',
            self::V18_PLUS => 'V18+',
            self::V19_MINUS => 'V19-',
            self::V19 => 'V19',
            self::V19_PLUS => 'V19+',
        };
    }
}