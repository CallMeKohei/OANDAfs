#/bin/bash

paket init
echo "nuget FSharp.Data" >> paket.dependencies
paket install

declare -a arr=(
    'fsharpc'
    '-a'
    '--nologo'
    '--simpleresolution'

    '-I:./packages/FSharp.Data/lib/net40/'
    '-r:FSharp.Data.DesignTime.dll'
    '-r:FSharp.Data.dll'

    'Oanda.fsx'

)
${arr[@]}
