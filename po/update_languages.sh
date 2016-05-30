#!/bin/sh

for language in `cat LINGUAS | grep -v "#"`; do
	echo $language
	intltool-update -d $language

	# Remove trailing commented out translations. This fails with unicode characters on OSX
	sed -E -i 's/^#~([áéíóú]|.)*//' $language.po
	sed -i -e :a -e '/^\n*$/{$d;N;};/\n$/ba' $language.po
done