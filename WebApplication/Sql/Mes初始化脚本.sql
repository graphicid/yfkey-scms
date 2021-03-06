GO
/****** 对象:  Table [dbo].[Mes_BomMstr]    脚本日期: 10/13/2011 13:48:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Mes_BomMstr](
	[Code] [varchar](50) NOT NULL,
	[Desc1] [varchar](255) NOT NULL,
	[Uom] [varchar](5) NULL,
	[Region] [varchar](50) NULL,
	[TFlag] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL CONSTRAINT [DF_MesBomMstr_IsActive]  DEFAULT ((1)),
 CONSTRAINT [PK_MesBomMstr] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[Mes_BomMstr]  WITH NOCHECK ADD  CONSTRAINT [FK_MesBomMstr_Region] FOREIGN KEY([Region])
REFERENCES [dbo].[Party] ([Code])
GO
ALTER TABLE [dbo].[Mes_BomMstr] CHECK CONSTRAINT [FK_MesBomMstr_Region]
GO
ALTER TABLE [dbo].[Mes_BomMstr]  WITH NOCHECK ADD  CONSTRAINT [FK_MesBomMstr_Uom] FOREIGN KEY([Uom])
REFERENCES [dbo].[Uom] ([Code])
GO
ALTER TABLE [dbo].[Mes_BomMstr] CHECK CONSTRAINT [FK_MesBomMstr_Uom]


GO
/****** 对象:  Table [dbo].[Mes_BomDet]    脚本日期: 10/13/2011 13:44:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Mes_BomDet](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Bom] [varchar](50) NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[Op] [int] NOT NULL,
	[Ref] [varchar](50) NULL,
	[StruType] [varchar](50) NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NULL,
	[Uom] [varchar](5) NOT NULL,
	[RateQty] [decimal](18, 8) NOT NULL,
	[ScrapPct] [decimal](18, 8) NOT NULL,
	[Priority] [int] NULL,
	[NeedPrint] [bit] NOT NULL,
	[Loc] [varchar](50) NULL,
	[HuLotSize] [int] NULL,
	[IsShipScan] [bit] NOT NULL CONSTRAINT [DF_MesBomDet_IsShipScan]  DEFAULT ((0)),
	[BackFlushMethod] [varchar](50) NULL,
	[IsActive] [bit] NOT NULL,
	
 CONSTRAINT [PK_MesBomDet] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[Mes_BomDet]  WITH NOCHECK ADD  CONSTRAINT [FK_MesBomDet_MesBomMstr] FOREIGN KEY([Bom])
REFERENCES [dbo].[Mes_BomMstr] ([Code])
GO
ALTER TABLE [dbo].[Mes_BomDet] CHECK CONSTRAINT [FK_MesBomDet_MesBomMstr]
GO
ALTER TABLE [dbo].[Mes_BomDet]  WITH NOCHECK ADD  CONSTRAINT [FK_MesBomDet_Item] FOREIGN KEY([Item])
REFERENCES [dbo].[Item] ([Code])
GO
ALTER TABLE [dbo].[Mes_BomDet] CHECK CONSTRAINT [FK_MesBomDet_Item]
GO
ALTER TABLE [dbo].[Mes_BomDet]  WITH CHECK ADD  CONSTRAINT [FK_MesBomDet_Location] FOREIGN KEY([Loc])
REFERENCES [dbo].[Location] ([Code])
GO
ALTER TABLE [dbo].[Mes_BomDet] CHECK CONSTRAINT [FK_MesBomDet_Location]
GO
ALTER TABLE [dbo].[Mes_BomDet]  WITH CHECK ADD  CONSTRAINT [FK_MesBomDet_Uom] FOREIGN KEY([Uom])
REFERENCES [dbo].[Uom] ([Code])
GO
ALTER TABLE [dbo].[Mes_BomDet] CHECK CONSTRAINT [FK_MesBomDet_Uom]

GO
/****** 对象:  Table [dbo].[Mes_ByMaterial]    脚本日期: 10/07/2011 17:09:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Mes_ByMaterial](
	[Id] [int] NOT NULL,
	[OrderNo] [varchar](50) NOT NULL,
	[TagNo] [varchar](50) NOT NULL,
	[CreateUser] [varchar](50) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[Item] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Mes_ByMaterial] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[Mes_ByMaterial]  WITH CHECK ADD  CONSTRAINT [FK_Mes_ByMaterial_ACC_User] FOREIGN KEY([CreateUser])
REFERENCES [dbo].[ACC_User] ([USR_Code])
GO
ALTER TABLE [dbo].[Mes_ByMaterial] CHECK CONSTRAINT [FK_Mes_ByMaterial_ACC_User]
GO
ALTER TABLE [dbo].[Mes_ByMaterial]  WITH CHECK ADD  CONSTRAINT [FK_Mes_ByMaterial_Item] FOREIGN KEY([Item])
REFERENCES [dbo].[Item] ([Code])
GO
ALTER TABLE [dbo].[Mes_ByMaterial] CHECK CONSTRAINT [FK_Mes_ByMaterial_Item]
GO
ALTER TABLE [dbo].[Mes_ByMaterial]  WITH CHECK ADD  CONSTRAINT [FK_Mes_ByMaterial_Mes_ByMaterial] FOREIGN KEY([Id])
REFERENCES [dbo].[Mes_ByMaterial] ([Id])
GO
ALTER TABLE [dbo].[Mes_ByMaterial] CHECK CONSTRAINT [FK_Mes_ByMaterial_Mes_ByMaterial]

GO
/****** 对象:  Table [dbo].[Mes_Shelf]    脚本日期: 08/11/2011 13:01:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Mes_Shelf](
	[Code] [varchar](50) NOT NULL,
	[ProdLine] [varchar](50) NOT NULL,
	[TagNo] [varchar](50) NOT NULL,
	[Capacity] [int] NOT NULL,
	[CurrentCartons] [int] NOT NULL,
	[OriginalCartonNo] [varchar](50) NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_StationShelf] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[Mes_Shelf]  WITH CHECK ADD  CONSTRAINT [FK_Mes_Shelf_FlowMstr] FOREIGN KEY([ProdLine])
REFERENCES [dbo].[FlowMstr] ([Code])
GO
ALTER TABLE [dbo].[Mes_Shelf] CHECK CONSTRAINT [FK_Mes_Shelf_FlowMstr]
GO
ALTER TABLE [dbo].[Mes_Shelf]  WITH CHECK ADD  CONSTRAINT [FK_Mes_Shelf_HuDet] FOREIGN KEY([OriginalCartonNo])
REFERENCES [dbo].[HuDet] ([HuId])
GO
ALTER TABLE [dbo].[Mes_Shelf] CHECK CONSTRAINT [FK_Mes_Shelf_HuDet]


GO
/****** 对象:  Table [dbo].[Mes_ShelfItem]    脚本日期: 08/11/2011 13:01:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Mes_ShelfItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Shelf] [varchar](50) NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_ShelfItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF


GO
/****** 对象:  Table [dbo].[Mes_ProdLineUser]    脚本日期: 08/11/2011 13:01:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Mes_ProdLineUser](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProdLine] [varchar](50) NOT NULL,
	[DeliveryUser] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Mes_ProdLineUser] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[Mes_ProdLineUser]  WITH CHECK ADD  CONSTRAINT [FK_Mes_ProdLineUser_ACC_User] FOREIGN KEY([DeliveryUser])
REFERENCES [dbo].[ACC_User] ([USR_Code])
GO
ALTER TABLE [dbo].[Mes_ProdLineUser] CHECK CONSTRAINT [FK_Mes_ProdLineUser_ACC_User]
GO
ALTER TABLE [dbo].[Mes_ProdLineUser]  WITH CHECK ADD  CONSTRAINT [FK_Mes_ProdLineUser_FlowMstr] FOREIGN KEY([ProdLine])
REFERENCES [dbo].[FlowMstr] ([Code])
GO
ALTER TABLE [dbo].[Mes_ProdLineUser] CHECK CONSTRAINT [FK_Mes_ProdLineUser_FlowMstr]