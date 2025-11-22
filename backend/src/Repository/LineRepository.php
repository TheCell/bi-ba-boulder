<?php

namespace App\Repository;

use App\Entity\Line;
use Doctrine\Bundle\DoctrineBundle\Repository\ServiceEntityRepository;
use Doctrine\Persistence\ManagerRegistry;
use Doctrine\DBAL\ParameterType;
use Symfony\Component\Uid\Uuid;

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
   public function findLinesByBlocId(Uuid $value): array
   {
       return $this->createQueryBuilder('l')
            ->andWhere('l.bloc = :val')
            ->setParameter('val', $value->toBinary(), ParameterType::BINARY)
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
