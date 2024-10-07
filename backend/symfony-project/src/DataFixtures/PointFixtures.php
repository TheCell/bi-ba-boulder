<?php

namespace App\DataFixtures;

use App\Entity\Point;
use Doctrine\Bundle\FixturesBundle\Fixture;
use Doctrine\Persistence\ObjectManager;

class PointFixtures extends Fixture
{
    public function load(ObjectManager $manager): void
    {
        $point1 = new Point();
        $point1->setX(-4.85421855159792);
        $point1->setY(2.75892462438469);
        $point1->setZ(-0.84770541503336);
        $manager->persist($point1);

        $point2 = new Point();
        $point2->setX(-5.6208964215916035);
        $point2->setY(3.746986553286824);
        $point2->setZ(-0.6169854902345677);
        $manager->persist($point2);

        $point3 = new Point();
        $point3->setX(-5.759726342438388);
        $point3->setY(4.264876456581907);
        $point3->setZ(-0.5953435383639133);
        $manager->persist($point3);

        $point4 = new Point();
        $point4->setX(-5.482444116670921);
        $point4->setY(4.880305862027428);
        $point4->setZ(-1.245281793845735);
        $manager->persist($point4);

        $point5 = new Point();
        $point5->setX(-5.211366255910471);
        $point5->setY(5.685785494666295);
        $point5->setZ(-1.7475199090809428);
        $manager->persist($point5);

        $point6 = new Point();
        $point6->setX(-5.038187349263503);
        $point6->setY(6.441635079133443);
        $point6->setZ(-1.3575449157703507);
        $manager->persist($point6);

        $point7 = new Point();
        $point7->setX(-4.6949815232454695);
        $point7->setY(7.225580663022901);
        $point7->setZ(-0.9360853014114401);
        $manager->persist($point7);

        $point8 = new Point();
        $point8->setX(-3.9267911729568223);
        $point8->setY(7.663158584055368);
        $point8->setZ(-0.3998278635016318);
        $manager->persist($point8);

        $manager->flush();

        $this->addReference('point-1', $point1);
        $this->addReference('point-2', $point2);
        $this->addReference('point-3', $point3);
        $this->addReference('point-4', $point4);
        $this->addReference('point-5', $point5);
        $this->addReference('point-6', $point6);
        $this->addReference('point-7', $point7);
        $this->addReference('point-8', $point8);
    }
}
