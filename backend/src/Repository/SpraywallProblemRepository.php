<?php

namespace App\Repository;

use App\Entity\SpraywallProblem;
use Doctrine\Bundle\DoctrineBundle\Repository\ServiceEntityRepository;
use Doctrine\Persistence\ManagerRegistry;

/**
 * @extends ServiceEntityRepository<SpraywallProblem>
 */
class SpraywallProblemRepository extends ServiceEntityRepository
{
    public function __construct(ManagerRegistry $registry)
    {
        parent::__construct($registry, SpraywallProblem::class);
    }

    /**
    * @return SpraywallProblem[] Returns an array of SpraywallProblem objects
    */
    public function findBySpraywallId($value): array
    {
        return $this->createQueryBuilder('s')
            ->andWhere('s.spraywall = :val')
            ->setParameter('val', $value)
            ->orderBy('s.id', 'ASC')
            ->setMaxResults(10)
            ->getQuery()
            ->getResult()
        ;
    }

    public function addProblem(SpraywallProblem $spraywallProblem): void
    {
        $entityManager = $this->getEntityManager();
        $entityManager->persist($spraywallProblem);
        $entityManager->flush();
    }

    public function removeProblem(SpraywallProblem $spraywallProblem): void
    {
        $entityManager = $this->getEntityManager();
        $entityManager->remove($spraywallProblem);
        $entityManager->flush();
    }

    //    public function findOneBySomeField($value): ?SpraywallProblem
    //    {
    //        return $this->createQueryBuilder('s')
    //            ->andWhere('s.exampleField = :val')
    //            ->setParameter('val', $value)
    //            ->getQuery()
    //            ->getOneOrNullResult()
    //        ;
    //    }
}
