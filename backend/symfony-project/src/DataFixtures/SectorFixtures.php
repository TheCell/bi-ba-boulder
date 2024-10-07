<?php

namespace App\DataFixtures;

use App\Entity\Sector;
use Doctrine\Bundle\FixturesBundle\Fixture;
use Doctrine\Common\DataFixtures\DependentFixtureInterface;
use Doctrine\Persistence\ObjectManager;

class SectorFixtures extends Fixture implements DependentFixtureInterface
{
    public function load(ObjectManager $manager): void
    {
        $sector = new Sector();
        $sector->setName('Test Sector');
        $sector->setDescription('This is a test sector.');
        $sector->addBloc($this->getReference('bloc-1'));
        $manager->persist($sector);

        $manager->flush();
    }

    public function getDependencies(): array
    {
        return [
            BlocFixtures::class,
        ];
    }
}
