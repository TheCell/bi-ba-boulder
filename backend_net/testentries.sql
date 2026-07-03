
INSERT INTO Sectors (Id, Name, Description, CreatedDate, CreatedUserId, Version)
VALUES ('957a8bf5-9197-4e3e-a10d-08de68da32a8', 'Sector 1', 'First sector', GETDATE(), '00000000-0000-0000-0000-000000000000', 1), 
       ('05e603dd-f9e7-4535-a10e-08de68da32a8', 'Sector 2', 'Second sector', GETDATE(), '00000000-0000-0000-0000-000000000000', 1)
       
INSERT INTO Blocs (
    Id, 
    Name, 
    Description, 
    BlocLowRes, 
    BlocMedRes, 
    BlocHighRes, 
    SectorId, 
    CreatedDate, 
    CreatedUserId, 
    UpdatedDate, 
    UpdatedUserId, 
    DeletedDate, 
    DeletedUserId, 
    Version
) VALUES 
(
    '019f08b6-7908-7052-8fba-c8880d9b46c6', 
    'Drytool Bloc', 
    'Es hat alles da', 
    'Blausee_Drytoolblock_LOD2.glb', 
    'Blausee_Drytoolblock_LOD1.glb', 
    'Blausee_Drytoolblock_LOD0.glb', 
    '05e603dd-f9e7-4535-a10e-08de68da32a8', 
    '2026-06-27 12:54:53.3900000', 
    '52fe4f3a-9a05-4718-9771-85ecb9b9ae40', 
    NULL, NULL, NULL, NULL, 
    1
),
(
    '019f08b6-64f1-71e1-af8f-ef3f51d2cc98', 
    'Big Bloc', 
    'Outdoor Test 2', 
    'Daone_HIS_0761_1.glb', 
    'Daone_HIS_0761_2.glb', 
    'Daone_HIS_0761_3.glb', 
    '957a8bf5-9197-4e3e-a10d-08de68da32a8', 
    '2026-06-27 12:54:53.3900000', 
    '52fe4f3a-9a05-4718-9771-85ecb9b9ae40', 
    NULL, NULL, NULL, NULL, 
    1
),
(
    '019f08ad-ff12-77ce-8e40-f1776adc2457', 
    'Small Bloc', 
    'Outdoor Test', 
    'Daone_HIS_0110_Cleanup_1.glb', 
    'Daone_HIS_0110_Cleanup_2.glb', 
    'Daone_HIS_0110_Cleanup_3.glb', 
    '957a8bf5-9197-4e3e-a10d-08de68da32a8', 
    '2026-06-27 12:52:41.7033333', 
    '52fe4f3a-9a05-4718-9771-85ecb9b9ae40', 
    NULL, NULL, NULL, NULL, 
    1
);
