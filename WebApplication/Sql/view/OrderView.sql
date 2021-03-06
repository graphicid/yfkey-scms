USE [sconit_yfk]
GO
/****** 对象:  View [dbo].[OrderView]    脚本日期: 09/02/2010 14:26:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[OrderView]
AS
SELECT DISTINCT Date, OrderNo, PLNo, Flow, OrderViewType, Status, PartyTo, CreateDate, StartDate, StartUser, CompleteDate, CompleteUser
FROM         (SELECT     CASE WHEN (substring(CONVERT(nvarchar(19), a.createdate, 120), 12, 2) < '07') THEN CONVERT(nvarchar(10), createdate - 1, 120) 
                                              ELSE CONVERT(nvarchar(10), createdate, 120) END AS Date, a.OrderNo, d.PLNo, a.Flow, CASE WHEN d .plno IS NULL 
                                              THEN '1' ELSE '0' END AS OrderViewType, a.Status, a.PartyTo, a.CreateDate, d.StartDate, d.StartUser, a.CompleteDate, 
                                              a.CompleteUser
                       FROM          dbo.OrderMstr AS a INNER JOIN
                                              dbo.OrderDet AS b ON a.OrderNo = b.OrderNo INNER JOIN
                                              dbo.OrderLocTrans AS c ON c.OrderDetId = b.Id AND c.IOType = 'OUT' LEFT OUTER JOIN
                                                  (SELECT     a.PLNo, a.Status, a.StartDate, a.StartUser, b.OrderLocTransId, b.Item, b.Qty AS plqty
                                                    FROM          dbo.PickListMstr AS a INNER JOIN
                                                                           dbo.PickListDet AS b ON a.PLNo = b.PLNo
                                                    WHERE      (a.PartyFrom = 'yfk-rw')) AS d ON c.Id = d.OrderLocTransId
                       WHERE      (a.Type = 'Transfer') AND (a.PartyFrom = 'YFK-RW') AND (a.PartyTo IN ('YFK-AB', 'YFK-SB', 'YFK-SW'))) AS m

GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
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
         Begin Table = "m"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 204
               Right = 234
            End
            DisplayFlags = 280
            TopColumn = 3
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1680
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'OrderView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'OrderView'