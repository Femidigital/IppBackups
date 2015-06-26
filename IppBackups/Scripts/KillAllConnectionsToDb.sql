declare @sql as varchar(20), @spid as int
select @spid = min(spid)  
from master..sysprocesses  
where dbid = db_id('ToBeReplaced') 
and spid != @@spid    
while (@spid is not null)begin    
print 'Killing process ' + cast(@spid as varchar) + ' ...'    
set @sql = 'kill ' + cast(@spid as varchar)    
exec (@sql)    
select         
@spid = min(spid)      
from         
master..sysprocesses      
where         
dbid = db_id('ToBeReplaced')         
and spid != @@spid
end 
print 'Process completed...'