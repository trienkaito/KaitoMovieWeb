using System.Net;

namespace MVC.ExtendNethods
{
    public static class AppExtends
    {
        public static void AddStatusCodePages(this WebApplication app) // or IApplicationBuilder
        {
            app.UseStatusCodePages(appError =>
            {
                appError.Run(async context =>
                {
                    var respone = context.Response;
                    var code = respone.StatusCode;

                    var content = @$"<!DOCTYPE html>
<html lang=""en"">

<head>

    <meta charset=""utf-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1, shrink-to-fit=no"">
    <meta name=""description"" content="""">
    <meta name=""author"" content="""">

       <title>Error {code}</title>

    <!-- Custom fonts for this template-->
    <link href=""/lib/font-awesome/css/all.min.css"" rel=""stylesheet"" type=""text/css"">
    <link
        href=""https://fonts.googleapis.com/css?family=Nunito:200,200i,300,300i,400,400i,600,600i,700,700i,800,800i,900,900i""
        rel=""stylesheet"">

    <!-- Custom styles for this template-->
    <link href=""/css/sb-admin-2.min.css"" rel=""stylesheet"">

</head>

<body id=""page-top"">

                    <!-- Begin Page Content -->
                <div class=""container-fluid"">

                    <!-- 404 Error Text -->
                    <div class=""text-center"">
                        <div class=""error mx-auto"" data-text=""{code}"">{code}</div>
                        <p class=""lead text-gray-800 mb-5"">{(HttpStatusCode)code}</p>
                        <p class=""text-gray-500 mb-0"">It looks like you found a glitch in the matrix...</p>
                        <a href=""home/index"">&larr; Back to Home</a>
                    </div>

                </div>
                <!-- /.container-fluid -->

    
    <!-- Bootstrap core JavaScript-->
    <script src=""/lib/jquery/dist/jquery.min.js""></script>
    <script src=""/lib/bootstrap/dist/js/bootstrap.bundle.min.js""></script>

    <!-- Core plugin JavaScript-->
    <script src=""/lib/jquery-easing/jquery.easing.min.js""></script>

    <!-- Custom scripts for all pages-->
    <script src=""/js/sb-admin-2.min.js""></script>

</body>

</html>";

                    await respone.WriteAsync(content);
                });
            });

        }
    }
}