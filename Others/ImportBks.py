#coding=utf8
import MySQLdb
import json, os, time, datetime



db_account = 'root'
db_password = 'root'
db_database = 'douban'
db_port = 3603
db = MySQLdb.connect('5758ead546603.sh.cdb.myqcloud.com', db_account, db_password, db_database,  charset="utf8", port=db_port)
cursor = db.cursor()
sql = ''				# SQL command

#----------------- Main -------------------#

book_keys = [ 'id', 'title', 'sub_title', 'origin_title', 'alt_title', 'publisher', 'isbn10', 'isbn13', 'pages', 'price', 'ebook_price', 'pubdate', 'summary', 'catalog', 'alt', 'image', 'ebook_url', 'url', 'binding' ]
images_keys = [ 'small', 'medium', 'large' ]
rating_keys = [ 'max', 'numRaters', 'average', 'min' ]

inserted_json = []

while( True ):
	for file in os.listdir('.'):
		if os.path.splitext(file)[1][1:] != 'json' or ( file in inserted_json ):
			continue
		inserted_json.append(file)
		print datetime.now()
		print file
		with open(file, 'r') as input:
			data = json.load(input, encoding='utf-8')
			for bk in data['books']:
				try:
					sql = " INSERT INTO `douban`.`books` (`id`, `title`, `sub_title`, `origin_title`, `alt_title`, `publisher`, `isbn10`, `isbn13`, `pages`, `price`, `ebook_price`, `pubdate`, `summary`, `catalog`, `alt`, `image`, `ebook_url`, `url`, `binding`) VALUES ('%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s'); " % tuple([ bk[key] if (key in bk) else '' for key in book_keys ])
					cursor.execute(sql)
				except MySQLdb.Error, e:
					print 'except :', str(e).decode('utf-8')
					continue		# next book
				
				try:
					if( 'images' in bk ):
						print 
						sql = " INSERT INTO `douban`.`images` (`id`, `small`, `medium`, `large`) VALUES ('%s', '%s', '%s', '%s') " % tuple([bk['id']]+[ bk['images'][key] if (key in bk['images']) else '' for key in images_keys ]  )
					cursor.execute(sql)
				except MySQLdb.Error, e:
					print 'except :', str(e).decode('utf-8')
					continue		# next book
				
				try:
					if( 'rating' in bk ):
						sql = " INSERT INTO `douban`.`ratings` (`id`, `max`, `numRaters`, `average`, `min`) VALUES ('%s', '%s', '%s', '%s', '%s') " % tuple([bk['id']]+[ bk['rating'][key] if (key in bk['rating']) else '' for key in rating_keys ]  )
					cursor.execute(sql)
				except MySQLdb.Error, e:
					print 'except :', str(e).decode('utf-8')
					continue		# next book
				
				try:
					if( 'author' in bk ):
						for au in bk['author']:
							sql = " INSERT INTO `douban`.`authors` (`id`, `author`) VALUES('%s', '%s')" % ( bk['id'], au )
							cursor.execute(sql)
				except MySQLdb.Error, e:
					print 'except :', str(e).decode('utf-8')
					continue		# next book
				
				try:
					if( 'tags' in bk ):
						for tg in bk['tags']:
							sql = " INSERT INTO `douban`.`tags` (`id`, `tag`, `count`)  VALUES('%s', '%s', '%s')" % ( bk['id'], tg['name'], tg['count'] )
							cursor.execute(sql)
				except MySQLdb.Error, e:
					print 'except :', str(e).decode('utf-8')
					continue		# next book
				
				try:
					if( 'author_intro' in bk ):
						tmp = ''
						if( isinstance(bk['author'], list) and len(bk['author']) > 0 ):
							tmp = bk['author'][0]
						else:
							tmp = bk['author']
						sql = " INSERT INTO `douban`.`author_intro` (`author`, `author_intro`) VALUES('%s', '%s') " % ( tmp, bk['author_intro'] )
						cursor.execute(sql)
				except MySQLdb.Error, e:
					print 'except :', str(e).decode('utf-8')
					continue		# next book
				
				try:
					if( 'translator' in bk ):
						for tsl in bk['translator']:
							sql = " INSERT INTO `douban`.`translators` (`id`, `translator`)  VALUES('%s', '%s')" % ( bk['id'], tsl )
							cursor.execute(sql)
				except MySQLdb.Error, e:
					print 'except :', str(e).decode('utf-8')
					continue		# next book
				
		db.commit()		# End_Open_File
	time.sleep(1200)
#----------------- EndMain -----------------#
cursor.close()
db.commit()
db.close()

