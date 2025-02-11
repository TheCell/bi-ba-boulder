<?php

namespace App\Repository;

use App\Entity\Line;
use Doctrine\Bundle\DoctrineBundle\Repository\ServiceEntityRepository;
use Doctrine\Persistence\ManagerRegistry;

/**
 * @extends ServiceEntityRepository<Line>
 */
class LineRepository extends ServiceEntityRepository
{
    public function __construct(ManagerRegistry $registry)
    {
        parent::__construct($registry, Line::class);
    }

   /**
    * @return Line[] Returns an array of Line objects
    */
   public function findLinesByBlocId($value): array
   {
       return $this->createQueryBuilder('l')
           ->andWhere('l.bloc = :val')
           ->setParameter('val', $value)
           ->orderBy('l.id', 'ASC')
           ->setMaxResults(100)
           ->leftJoin('l.bloc', 'b')
           ->getQuery()
           ->getResult();
   }

//    public function findOneBySomeField($value): ?Line
//    {
//        return $this->createQueryBuilder('l')
//            ->andWhere('l.exampleField = :val')
//            ->setParameter('val', $value)
//            ->getQuery()
//            ->getOneOrNullResult()
//        ;
//    }
}
