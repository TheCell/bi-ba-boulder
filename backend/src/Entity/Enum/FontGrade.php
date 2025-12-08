<?php

namespace App\Entity\Enum;

enum FontGrade: int
{
    case ONE_MINUS = 0;
    case ONE = 1;
    case ONE_PLUS = 2;
    case TWO_MINUS = 3;
    case TWO = 4;
    case TWO_PLUS = 5;
    case THREE_MINUS = 6;
    case THREE = 7;
    case THREE_PLUS = 8;
    case FOUR_MINUS = 9;
    case FOUR = 10;
    case FOUR_PLUS = 11;
    case FIVE_MINUS = 12;
    case FIVE = 13;
    case FIVE_PLUS = 14;
    case SIX_A_MINUS = 15;
    case SIX_A = 16;
    case SIX_A_PLUS = 17;
    case SIX_B_MINUS = 18;
    case SIX_B = 19;
    case SIX_B_PLUS = 20;
    case SIX_C_MINUS = 21;
    case SIX_C = 22;
    case SIX_C_PLUS = 23;
    case SEVEN_A_MINUS = 24;
    case SEVEN_A = 25;
    case SEVEN_A_PLUS = 26;
    case SEVEN_B_MINUS = 27;
    case SEVEN_B = 28;
    case SEVEN_B_PLUS = 29;
    case SEVEN_C_MINUS = 30;
    case SEVEN_C = 31;
    case SEVEN_C_PLUS = 32;
    case EIGHT_A_MINUS = 33;
    case EIGHT_A = 34;
    case EIGHT_A_PLUS = 35;
    case EIGHT_B_MINUS = 36;
    case EIGHT_B = 37;
    case EIGHT_B_PLUS = 38;
    case EIGHT_C_MINUS = 39;
    case EIGHT_C = 40;
    case EIGHT_C_PLUS = 41;
    case NINE_A_MINUS = 42;
    case NINE_A = 43;
    case NINE_A_PLUS = 44;
    case NINE_B_MINUS = 45;
    case NINE_B = 46;
    case NINE_B_PLUS = 47;
    case NINE_C_MINUS = 48;
    case NINE_C = 49;
    case NINE_C_PLUS = 50;

    // public static function getEnumByName(string $name): ?FontGrade
    // {
    //     foreach (FontGrade::cases() as $case) {
    //         if (strcasecmp($case->value, $name) === 0) {
    //             return $case;
    //         }
    //     }

    //     return null;
    // }
    public function getValue(): int
    {
        return $this->value;
    }

    public function GetName(): string
    {
        return match($this) {
            self::ONE_MINUS => '1-',
            self::ONE => '1',
            self::ONE_PLUS => '1+',
            self::TWO_MINUS => '2-',
            self::TWO => '2',
            self::TWO_PLUS => '2+',
            self::THREE_MINUS => '3-',
            self::THREE => '3',
            self::THREE_PLUS => '3+',
            self::FOUR_MINUS => '4-',
            self::FOUR => '4',
            self::FOUR_PLUS => '4+',
            self::FIVE_MINUS => '5-',
            self::FIVE => '5',
            self::FIVE_PLUS => '5+',
            self::SIX_A_MINUS => '6A-',
            self::SIX_A => '6A',
            self::SIX_A_PLUS => '6A+',
            self::SIX_B_MINUS => '6B-',
            self::SIX_B => '6B',
            self::SIX_B_PLUS => '6B+',
            self::SIX_C_MINUS => '6C-',
            self::SIX_C => '6C',
            self::SIX_C_PLUS => '6C+',
            self::SEVEN_A_MINUS => '7A-',
            self::SEVEN_A => '7A',
            self::SEVEN_A_PLUS => '7A+',
            self::SEVEN_B_MINUS => '7B-',
            self::SEVEN_B => '7B',
            self::SEVEN_B_PLUS => '7B+',
            self::SEVEN_C_MINUS => '7C-',
            self::SEVEN_C => '7C',
            self::SEVEN_C_PLUS => '7C+',
            self::EIGHT_A_MINUS => '8A-',
            self::EIGHT_A => '8A',
            self::EIGHT_A_PLUS => '8A+',
            self::EIGHT_B_MINUS => '8B-',
            self::EIGHT_B => '8B',
            self::EIGHT_B_PLUS => '8B+',
            self::EIGHT_C_MINUS => '8C-',
            self::EIGHT_C => '8C',
            self::EIGHT_C_PLUS => '8C+',
            self::NINE_A_MINUS => '9A-',
            self::NINE_A => '9A',
            self::NINE_A_PLUS => '9A+',
            self::NINE_B_MINUS => '9B-',
            self::NINE_B => '9B',
            self::NINE_B_PLUS => '9B+',
            self::NINE_C_MINUS => '9C-',
            self::NINE_C => '9C',
            self::NINE_C_PLUS => '9C+',
        };
    }
}