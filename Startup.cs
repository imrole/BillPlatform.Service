using BillPlatform.BLL;
using BillPlatform.DAL;
using BillPlatform.IBLL;
using BillPlatform.IDAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace BillPlatform.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<EFContext>();
            services.AddScoped<ITestDAL, TestDAL>();
            services.AddScoped<ITestBLL, TestBLL>();
            services.AddScoped<IIndUserDAL, IndUserDAL>();
            services.AddScoped<IIndUserBLL,IndUserBLL>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "����ƽ̨",
                    Version = "V1.0.1",
                    Description = "����API"
                });

                //����apiע��
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath, true); //��ӿ�������ע�ͣ�true��ʾ��ʾ������ע�ͣ�
                //����swaggerȨ��
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                //���÷���
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme()
                {
                    Description = "���Bearer��һ���ո�",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
            });
            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    //��֤��Կ
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["JwtTokenManagement:secret"])),

                    //��֤������
                    ValidateIssuer = true,
                    ValidIssuer = Configuration["JwtTokenManagement:issuer"],

                    //��֤������
                    ValidateAudience = true,
                    ValidAudience = Configuration["JwtTokenManagement:audience"],

                    //��֤����ʱ��
                    RequireExpirationTime = true,

                    //��֤��������
                    ValidateLifetime = true,

                    //�������ʱ�䣬��ʹ�����˹���ʱ��ҲҪ���ǹ���ʱ��+����
                    ClockSkew = TimeSpan.Zero
                };
            });
            //���json����
            services.AddControllersWithViews().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            });
            //���viewbag����
            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BillPlatform.Service v1"));
            }

            app.UseRouting();

            //��֤�м��
            app.UseAuthentication();
            //��Ȩ�м��
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
