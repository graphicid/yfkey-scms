USE [sconit]
GO
/****** 对象:  View [dbo].[InspectDetView]    脚本日期: 08/26/2010 11:41:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[InspectDetView]
AS
SELECT     dbo.InspectDet.InspNo, dbo.LocationLotDet.Item, dbo.InspectDet.LocFrom, SUM(dbo.InspectDet.InspQty - ISNULL(dbo.InspectDet.QualifyQty, 0) 
                      - ISNULL(dbo.InspectDet.RejectQty, 0)) AS RemainQty, dbo.InspectDet.LocTo, MAX(dbo.InspectDet.Id) AS Id
FROM         dbo.InspectDet INNER JOIN
                      dbo.LocationLotDet ON dbo.InspectDet.LocLotDetId = dbo.LocationLotDet.Id INNER JOIN
                      dbo.InspectMstr ON dbo.InspectDet.InspNo = dbo.InspectMstr.InspNo
WHERE     (dbo.InspectMstr.Status = 'Create') AND (dbo.InspectDet.InspQty - ISNULL(dbo.InspectDet.QualifyQty, 0) - ISNULL(dbo.InspectDet.RejectQty, 0) > 0)
GROUP BY dbo.InspectDet.InspNo, dbo.LocationLotDet.Item, dbo.InspectDet.LocFrom, dbo.InspectDet.LocTo

GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[50] 4[19] 2[19] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "InspectDet"
            Begin Extent = 
               Top = 0
               Left = 337
               Bottom = 225
               Right = 510
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "LocationLotDet"
            Begin Extent = 
               Top = 26
               Left = 54
               Bottom = 252
               Right = 247
            End
            DisplayFlags = 280
            TopColumn = 1
         End
         Begin Table = "InspectMstr"
            Begin Extent = 
               Top = 8
               Left = 600
               Bottom = 241
               Right = 766
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'InspectDetView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'InspectDetView'