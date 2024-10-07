<?php

namespace App\DataFixtures;

use App\Entity\Bloc;
use Doctrine\Bundle\FixturesBundle\Fixture;
use Doctrine\Common\DataFixtures\DependentFixtureInterface;
use Doctrine\Persistence\ObjectManager;

class BlocFixtures extends Fixture implements DependentFixtureInterface
{
    public function load(ObjectManager $manager): void
    {
        $bloc = new Bloc();
        $bloc->setName('Test Bloc');
        $bloc->setDescription('This is a test bloc.');
        $bloc->addBoulderLine($this->getReference('line-1'));
        $manager->persist($bloc);

        $manager->flush();

        $this->addReference('bloc-1', $bloc);
    }

    public function getDependencies(): array
    {
        return [
            LineFixtures::class,
        ];
    }
}
