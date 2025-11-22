<?php

namespace App\Repository;

use App\Entity\Bloc;
use Doctrine\Bundle\DoctrineBundle\Repository\ServiceEntityRepository;
use Doctrine\DBAL\ParameterType;
use Doctrine\Persistence\ManagerRegistry;
use Symfony\Component\Uid\Uuid;

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
       public function findBySectorId(Uuid $value): array
       {
            return $this->createQueryBuilder('b')
                ->andWhere('b.sector = :val')
                ->setParameter('val', $value->toBinary(), ParameterType::BINARY)
                ->orderBy('b.id', 'ASC')
                ->setMaxResults(100)
                ->getQuery()
                ->getResult()
            ;
       }

       public function findById(Uuid $value): ?Bloc
       {
            return $this->createQueryBuilder('b')
                ->andWhere('b.id = :val')
                ->setParameter('val', $value->toBinary(), ParameterType::BINARY)
                ->getQuery()
                ->getOneOrNullResult()
           ;
       }
}
