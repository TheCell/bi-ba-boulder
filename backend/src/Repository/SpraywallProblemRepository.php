<?php

namespace App\Repository;

use App\Entity\SpraywallProblem;
use Doctrine\Bundle\DoctrineBundle\Repository\ServiceEntityRepository;
use Doctrine\Persistence\ManagerRegistry;
use Doctrine\DBAL\ParameterType;
use Symfony\Component\Uid\Uuid;

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
    public function findBySpraywallId(Uuid $value): array
    {
        return $this->createQueryBuilder('s')
            ->andWhere('s.spraywall = :val')
            ->setParameter('val', $value->toBinary(), ParameterType::BINARY)
            ->orderBy('s.id', 'ASC')
            ->setMaxResults(10)
            ->getQuery()
            ->getResult()
        ;
    }

    
    /**
    * @return SpraywallProblem Returns an array of Spraywall objects
    */
    public function findById($value): ?SpraywallProblem
    {
        return $this->createQueryBuilder('s')
            ->andWhere('s.id = :val')
            ->setParameter('val', $value->toBinary(), ParameterType::BINARY)
            ->orderBy('s.id', 'ASC')
            ->getQuery()
            ->getOneOrNullResult()
        ;
    }

    // public function problemCount(Uuid $spraywallId): int
    // {
    //     $qb = $this->createQueryBuilder('s')
    //         ->select('COUNT(s.id)')
    //         ->andWhere('s.spraywall = :spraywallId')
    //         ->setParameter('spraywallId', $spraywallId->toBinary(), ParameterType::BINARY);

    //     return (int) $qb->getQuery()->getSingleScalarResult();
    // }

    public function filterByCriteria(Uuid $spraywallId, int $pageSize, int $offset, array $criteria): array
    {
        // todo filter for nullable grade
        $qb = $this->createQueryBuilder('s');

        $qb->andWhere('s.spraywall = :spraywallId')
           ->setParameter('spraywallId', $spraywallId->toBinary(), ParameterType::BINARY);

        if (isset($criteria['gradeMin'])) {
            $qb->andWhere('s.FontGrade >= :gradeMin')
               ->setParameter('gradeMin', $criteria['gradeMin']);
        }

        if (isset($criteria['gradeMax'])) {
            $qb->andWhere('s.FontGrade <= :gradeMax')
               ->setParameter('gradeMax', $criteria['gradeMax']);
        }

        if (isset($criteria['name'])) {
            $qb->andWhere('s.name LIKE :name')
               ->setParameter('name', '%' . $criteria['name'] . '%');
        }

        if (isset($criteria['createdBy'])) {
            $qb->andWhere('s.CreatedBy = :createdBy')
               ->setParameter('createdBy', $criteria['createdBy']);
        }

        if (isset($criteria['dateOrder'])) {
            $order = strtolower($criteria['dateOrder']) === 'asc' ? 'ASC' : 'DESC';
            $qb->orderBy('s.CreatedDate', $order);
        }

        $totalCount = (clone $qb)
            ->select('COUNT(s.id)')
            ->resetDQLPart('orderBy')
            ->getQuery()
            ->getSingleScalarResult();

        $qb->setFirstResult($offset)
            ->setMaxResults($pageSize);

        $filteredResult = $qb->getQuery()->getResult();
        
        return [$filteredResult, $totalCount];
    }

    public function addProblem(SpraywallProblem $spraywallProblem): void
    {
        $entityManager = $this->getEntityManager();
        $entityManager->persist($spraywallProblem);
        $entityManager->flush();
    }

    public function updateProblem(): void
    {
        $entityManager = $this->getEntityManager();
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
