<?php
namespace App\DTO;

/**
 * Default error DTO for API error responses.
 */
class ErrorDto
{
    public function __construct(public string $error, public ?array $errors)
    { }
}
