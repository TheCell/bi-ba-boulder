<?php

namespace App\Repository;

use App\Entity\Bloc;
use Doctrine\Bundle\DoctrineBundle\Repository\ServiceEntityRepository;
use Doctrine\Persistence\ManagerRegistry;

/**
 * @extends ServiceEntityRepository<Bloc>
 */
class BlocRepository extends ServiceEntityRepository
{
    public function __construct(ManagerRegistry $registry)
    {
        parent::__construct($registry, Bloc::class);
    }

       /**
        * @return Bloc[] Returns an array of Bloc objects
        */
       public function findBySectorId($value): array
       {
           return $this->createQueryBuilder('b')
               ->andWhere('b.sector = :val')
               ->setParameter('val', $value)
               ->orderBy('b.id', 'ASC')
               ->setMaxResults(100)
               ->getQuery()
               ->getResult()
           ;
       }

       public function findById($value): ?Bloc
       {
           return $this->createQueryBuilder('b')
               ->andWhere('b.id = :val')
               ->setParameter('val', $value)
               ->getQuery()
               ->getOneOrNullResult()
           ;
       }
}
