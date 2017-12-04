echo \ "
 GIT METADATA
 ------------
     GIT_COMMIT ......................... : $GIT_COMMIT 
     GIT_PREVIOUS_COMMIT ................ : $GIT_PREVIOUS_COMMIT 
     GIT_PREVIOUS_SUCCESSFUL_COMMIT ..... : $GIT_PREVIOUS_SUCCESSFUL_COMMIT
     GIT_BRANCH ......................... : $GIT_BRANCH
     GIT_LOCAL_BRANCH ................... : $GIT_LOCAL_BRANCH
     GIT_URL ............................ : $GIT_URL
     GIT_COMMITTER_NAME ................. : $GIT_COMMITTER_NAME
     GIT_COMMITTER_EMAIL ................ : $GIT_COMMITTER_EMAIL
     GIT_AUTHOR_NAME .................... : $GIT_AUTHOR_NAME
     GIT_AUTHOR_EMAIL ................... : $GIT_AUTHOR_EMAIL
"

export PATH="/opt/mono/4.4.1.0/bin:$PATH"
echo MONO VERSION `mono --version | head -1`
echo _________________________________
bash -e teamcity-build.sh
