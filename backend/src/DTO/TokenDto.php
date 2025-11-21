<?php
namespace App\DTO;

/**
 * DTO for JWT token response.
 */
class TokenDto
{
    public function __construct(public string $token)
    { }
}
