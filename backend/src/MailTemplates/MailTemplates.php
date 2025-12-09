<?php
namespace App\MailTemplates;

final class MailTemplates
{
    private const TEMPLATEDIRECTORY = __DIR__ . DIRECTORY_SEPARATOR . 'Html';
    
    public function getVerifyMailTemplate(array $variables): string
    {
        $template = file_get_contents(self::TEMPLATEDIRECTORY . DIRECTORY_SEPARATOR . 'index.html');
        foreach($variables as $key => $value)
        {
            $template = str_replace('{{ '.$key.' }}', $value, $template);
        }

        return $template;
    }
}