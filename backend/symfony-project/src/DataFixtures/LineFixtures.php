<?php

namespace App\DataFixtures;

use App\Entity\Line;
use Doctrine\Bundle\FixturesBundle\Fixture;
use Doctrine\Common\DataFixtures\DependentFixtureInterface;
use Doctrine\Persistence\ObjectManager;

class LineFixtures extends Fixture implements DependentFixtureInterface
{
    public function load(ObjectManager $manager): void
    {
        $line1 = new Line();
        $line1->setIdentifier('A');
        $line1->setName('Test line');
        $line1->setDescription('This is a test bloc.');
        $line1->setColor('#842c36');
        $line1->addPoint($this->getReference('point-1'));
        $line1->addPoint($this->getReference('point-2'));
        $line1->addPoint($this->getReference('point-3'));
        $line1->addPoint($this->getReference('point-4'));
        $line1->addPoint($this->getReference('point-5'));
        $line1->addPoint($this->getReference('point-6'));
        $line1->addPoint($this->getReference('point-7'));
        $line1->addPoint($this->getReference('point-8'));
        $manager->persist($line1);

        $manager->flush();

        $this->addReference('line-1', $line1);
    }

    public function getDependencies(): array
    {
        return [
            PointFixtures::class,
        ];
    }
}
