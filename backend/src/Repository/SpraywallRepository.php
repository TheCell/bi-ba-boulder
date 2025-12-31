<?php

namespace App\Repository;

use App\Entity\Spraywall;
use Doctrine\Bundle\DoctrineBundle\Repository\ServiceEntityRepository;
use Doctrine\Persistence\ManagerRegistry;
use Doctrine\DBAL\ParameterType;
use Symfony\Component\Uid\Uuid;

/**
 * @extends ServiceEntityRepository<Spraywall>
 */
class SpraywallRepository extends ServiceEntityRepository
{
    public function __construct(ManagerRegistry $registry)
    {
        parent::__construct($registry, Spraywall::class);
    }

    /**
    * @return Spraywall Returns an array of Spraywall objects
    */
    public function findById($value): ?Spraywall
    {
        return $this->createQueryBuilder('s')
            ->andWhere('s.id = :val')
            ->setParameter('val', $value->toBinary(), ParameterType::BINARY)
            ->orderBy('s.id', 'ASC')
            ->getQuery()
            ->getOneOrNullResult()
        ;
    }

    // /**
    // * @return SpraywallProblem[] Returns an array of SpraywallProblem objects
    // */
    // public function findBySpraywallId(Uuid $value): array
    // {
    //     return $this->createQueryBuilder('s')
    //         ->andWhere('s.spraywall = :val')
    //         ->setParameter('val', $value->toBinary(), ParameterType::BINARY)
    //         ->orderBy('s.id', 'ASC')
    //         ->setMaxResults(10)
    //         ->getQuery()
    //         ->getResult()
    //     ;
    // }
    //    public function findOneBySomeField($value): ?Spraywall
    //    {
    //        return $this->createQueryBuilder('s')
    //            ->andWhere('s.exampleField = :val')
    //            ->setParameter('val', $value)
    //            ->getQuery()
    //            ->getOneOrNullResult()
    //        ;
    //    }
}
