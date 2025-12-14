SOLUTION_NAME = SimpleHttpServer
SOLUTION_FILE = $(SOLUTION_NAME).sln
MAIN_PROJECT_FILE = $(SOLUTION_NAME)\$(SOLUTION_NAME).csproj
PROJECT_DIRS = $(SOLUTION_NAME)
ARTIFACTS_BASEDIR = Artifacts
ARTIFACTS_SUBDIR_BASENAME = $(SOLUTION_NAME)
ARTIFACTS_BASENAME = $(SOLUTION_NAME)
BUILD_CONFIG = Release
TARGET_NET10 = net10.0
TARGET_NFW481 = net481
SINGLE_SUFFIX = -single
AOT_SUFFIX = -aot
RM = del /F /Q
RMDIR = rmdir /S /Q


all: build

build:
	dotnet build -c $(BUILD_CONFIG) $(MAIN_PROJECT_FILE)

restore:
	dotnet restore $(SOLUTION_FILE)

deploy: deploy-$(TARGET_NET10)

deploy$(SINGLE_SUFFIX): deploy-$(TARGET_NET10)$(SINGLE_SUFFIX)

deploy$(AOT_SUFFIX): deploy-$(TARGET_NET10)$(AOT_SUFFIX)

deploy-$(TARGET_NET10):
	-dotnet publish -c $(BUILD_CONFIG) -f $(TARGET_NET10) --no-self-contained \
		-p:PublishDir=..\$(ARTIFACTS_BASEDIR)\$(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NET10) \
		-p:PublishTrimmed=false \
		-p:PublishAot=false \
		$(MAIN_PROJECT_FILE)
	-$(RM) $(ARTIFACTS_BASEDIR)\$(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NET10)\*.pdb \
		$(ARTIFACTS_BASEDIR)\$(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NET10)\*.xml \
		$(ARTIFACTS_BASENAME)-$(TARGET_NET10).zip 2>NUL
	cd $(ARTIFACTS_BASEDIR)
	powershell Compress-Archive -Path $(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NET10) -DestinationPath ..\$(ARTIFACTS_BASENAME)-$(TARGET_NET10).zip
	cd $(MAKEDIR)

deploy-$(TARGET_NET10)$(SINGLE_SUFFIX):
	-dotnet publish -c $(BUILD_CONFIG) -f $(TARGET_NET10) --self-contained \
		-p:PublishDir=..\$(ARTIFACTS_BASEDIR)\$(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NET10)$(SINGLE_SUFFIX) \
		-p:PublishAot=false \
		-p:PublishSingleFile=true \
		-p:PublishReadyToRun=true \
		$(MAIN_PROJECT_FILE)
	-$(RM) $(ARTIFACTS_BASEDIR)\$(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NET10)$(SINGLE_SUFFIX)\*.pdb \
		$(ARTIFACTS_BASEDIR)\$(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NET10)$(SINGLE_SUFFIX)\*.xml \
		$(ARTIFACTS_BASENAME)-$(TARGET_NET10)$(SINGLE_SUFFIX).zip 2>NUL
	cd $(ARTIFACTS_BASEDIR)
	powershell Compress-Archive -Path $(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NET10)$(SINGLE_SUFFIX) -DestinationPath ..\$(ARTIFACTS_BASENAME)-$(TARGET_NET10)$(SINGLE_SUFFIX).zip
	cd $(MAKEDIR)

deploy-$(TARGET_NET10)$(AOT_SUFFIX):
	-dotnet publish -c $(BUILD_CONFIG) -f $(TARGET_NET10) --self-contained \
		-p:PublishDir=..\$(ARTIFACTS_BASEDIR)\$(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NET10)$(AOT_SUFFIX) \
		-p:OptimizationPreference=Speed \
		-p:InvariantGlobalization=true \
		-p:StripSymbols=true \
		-p:DebuggerSupport=false \
		-p:EnableUnsafeBinaryFormatterSerialization=false \
		-p:EventSourceSupport=false \
		-p:HttpActivityPropagationSupport=false \
		-p:IlcDisableReflection=true \
		-p:IlcFoldIdenticalMethodBodies=true \
		-p:IlcGenerateCompleteTypeMetadata=false \
		-p:IlcGenerateStackTraceData=false \
		-p:IlcOptimizationPreference=Speed \
		-p:MetadataUpdaterSupport=false \
		-p:RootAllApplicationAssemblies=false \
		-p:UseSystemResourceKeys=true \
		$(MAIN_PROJECT_FILE)
	-$(RM) $(ARTIFACTS_BASEDIR)\$(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NET10)$(AOT_SUFFIX)\*.pdb \
		$(ARTIFACTS_BASEDIR)\$(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NET10)$(AOT_SUFFIX)\*.xml \
		$(ARTIFACTS_BASENAME)-$(TARGET_NET10)$(AOT_SUFFIX).zip 2>NUL
	cd $(ARTIFACTS_BASEDIR)
	powershell Compress-Archive -Path $(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NET10)$(AOT_SUFFIX) -DestinationPath ..\$(ARTIFACTS_BASENAME)-$(TARGET_NET10)$(AOT_SUFFIX).zip
	cd $(MAKEDIR)

deploy-$(TARGET_NFW481):
	-dotnet publish -c $(BUILD_CONFIG) -f $(TARGET_NFW481) --no-self-contained \
		-p:PublishDir=..\$(ARTIFACTS_BASEDIR)\$(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NFW481) \
		-p:PublishTrimmed=false \
		-p:PublishAot=false \
		$(MAIN_PROJECT_FILE)
	-$(RM) $(ARTIFACTS_BASEDIR)\$(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NFW481)\*.pdb \
		$(ARTIFACTS_BASEDIR)\$(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NFW481)\*.xml \
		$(ARTIFACTS_BASENAME)-$(TARGET_NFW481).zip 2>NUL
	cd $(ARTIFACTS_BASEDIR)
	powershell Compress-Archive -Path $(ARTIFACTS_SUBDIR_BASENAME)-$(TARGET_NFW481) -DestinationPath ..\$(ARTIFACTS_BASENAME)-$(TARGET_NFW481).zip
	cd $(MAKEDIR)

clean:
	-for %%d in ( $(PROJECT_DIRS) ) do @( \
		@$(RMDIR) %%d\bin %%d\obj 2>NUL \
	)

distclean: clean
	-$(RMDIR) $(ARTIFACTS_BASEDIR) 2>NUL
	-$(RM) $(ARTIFACTS_BASENAME)-*.zip 2>NUL
