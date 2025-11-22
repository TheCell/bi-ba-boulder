<?php

namespace App\EventSubscriber;

use Symfony\Component\HttpKernel\Event\RequestEvent;
use Symfony\Component\RateLimiter\RateLimiterFactory;
use Symfony\Component\EventDispatcher\EventSubscriberInterface;
use Symfony\Component\HttpKernel\Exception\TooManyRequestsHttpException;

class RateLimiterSubscriber implements EventSubscriberInterface
{
    public function __construct(
        private RateLimiterFactory $anonymousApiLimiter,
        private RateLimiterFactory $registrationApiLimiter,
        private RateLimiterFactory $adminApiLimiter)
    {
        // $this->loginApiLimiter = $loginApiLimiter;
    }

    public static function getSubscribedEvents(): array
    {
        return [
            RequestEvent::class => 'onKernelRequest',
        ];
    }

    public function onKernelRequest(RequestEvent $event): void
    {
        $request = $event->getRequest();
        $path = $request->getPathInfo();
        
        if (strpos($path, '/api/register') !== false) {
            $limiter = $this->registrationApiLimiter->create($request->getClientIp());

            if (!$limiter->consume(1)->isAccepted()) {
                throw new TooManyRequestsHttpException();
            }
        } else if (strpos($path, '/api/admin') !== false) {
            $limiter = $this->adminApiLimiter->create($request->getClientIp());

            if (!$limiter->consume(1)->isAccepted()) {
                throw new TooManyRequestsHttpException();
            }
        } else if (strpos($path, '/api/') !== false) {
            $limiter = $this->anonymousApiLimiter->create($request->getClientIp());

            if (!$limiter->consume(1)->isAccepted()) {
                throw new TooManyRequestsHttpException();
            }
        }
    }
}