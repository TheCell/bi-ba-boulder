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

    public function getFeedbackTemplate(string $userMail, string $feedback): string
    {
        $template = file_get_contents(self::TEMPLATEDIRECTORY . DIRECTORY_SEPARATOR . 'feedback.html');
        $template = str_replace('{{ userMail }}', $userMail, $template);
        $template = str_replace('{{ feedback }}', $feedback, $template);

        return $template;
    }
}