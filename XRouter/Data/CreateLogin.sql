--SELECT sid FROM master.sys.syslogins s where s.name = 'XRouter_AccessDB'
--0x8FE5391BED35F446932698F0820D5E3C


CREATE LOGIN XRouter_AccessDB WITH PASSWORD = 'XRouter', SID=0x8FE5391BED35F446932698F0820D5E3C
GO