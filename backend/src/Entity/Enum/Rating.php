<?php

namespace App\Entity\Enum;

enum Rating: int
{
    case ONE = 1;
    case TWO = 2;
    case THREE = 3;
    case FOUR = 4;
    case FIVE = 5;
}