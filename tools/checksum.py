import subprocess
import shlex
import os


def checksum_changed(checksum_dir):
    md5, currentmd5 = read_checksum(checksum_dir)
    return md5 != currentmd5


def read_checksum(checksum_dir):
    md5file = _get_checksum_file(checksum_dir)
    if os.path.exists(md5file):
        with open(md5file) as f:
            md5 = f.read()
        print "Checksum: %s" % md5
    else:
        md5 = None
    currentmd5 = _compute_checksum(checksum_dir)
    if (currentmd5 == md5):
        print "Content didn't change, skipping"
    else:
        print "Checksum changed: %s" % currentmd5
    return (md5, currentmd5)


def save_checksum(checksum_dir):
    md5file = _get_checksum_file(checksum_dir)
    currentmd5 = _compute_checksum(checksum_dir)
    with open(md5file, 'w+') as f:
        f.write(currentmd5)


def _compute_checksum(checksum_dir):
    return subprocess.check_output(
            shlex.split('VAS/tools/compute_checksum.sh %s' % checksum_dir)).split('\n')[0]


def _get_checksum_file(checksum_dir):
    return '%s/checksum' % checksum_dir
