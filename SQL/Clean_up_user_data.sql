  DECLARE @user varchar(30);
  SET @user = 'jakejgordon3@gmail.com';
  DELETE FROM Player WHERE ApplicationUserId IN (SELECT Id FROM AspNetUsers WHERE email = @user)
  DELETE FROM GamingGroupInvitation WHERE RegisteredUserId IN (SELECT Id FROM AspNetUsers WHERE email = @user)
  DELETE FROM GamingGroup Where OwningUserId IN (SELECT Id FROM AspNetUsers WHERE email = @user)
  DELETE FROM AspNetUsers WHERE email = @user