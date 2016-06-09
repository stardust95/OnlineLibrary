#coding=utf-8
import requests, json, urllib, sys, time, os, Queue

url = [ "https://api.douban.com/v2/book/search?count=1000&tag=",
        "https://api.douban.com/v2/book/search?count=1000&start=100&tag="]

reload(sys)
sys.setdefaultencoding('utf8')

# cookies =
headers = {
'Cookie' : 'bid="4JSIWRr9vEc"; ll="118172"; gr_user_id=e560cad7-1de3-4429-90f0-d6c96d627cf4; ct=y; viewed="1231692_1223070_3619896_4114150_19986436"; regfromurl=https://www.douban.com/doulist/126455/; regfromtitle=%E8%AE%A1%E7%AE%97%E6%9C%BA%E7%B3%BB%E7%BB%9F%E3%80%81%E5%86%85%E6%A0%B8%E5%AD%A6%E4%B9%A0; dbcl2="133823011:XHQaM/eb1dQ"; ck="Bq4A"; push_noty_num=0; push_doumail_num=1; __utma=30149280.2095284842.1457067259.1459994375.1460006523.22; __utmc=30149280; __utmz=30149280.1460006523.22.20.utmcsr=google|utmccn=(organic)|utmcmd=organic|utmctr=(not%20provided); __utmv=30149280.13382',
'User-Agent' : 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.110 Safari/537.36'
}

def search_tag(tag, flag):
        tmpurl = url[flag] + urllib.quote(tag.encode('utf-8'))
        print tmpurl
        # if( depth > 5 ):
                # return
        global mark_tags
        mark_tags.append(tag)
        r = requests.get(tmpurl, headers=headers)
        with open(logfile, 'a') as output:
                output.write(tag+':'+str(r.status_code)+'\n')
        print tag.decode('utf-8')+':'+str(r.status_code)
        # print r.status_code
        if( r.status_code == 200 ):
                with open(tag.decode('utf-8')+'.json', 'w') as output:
                        output.write(r.text)
                for bk in json.loads(r.text)['books']:
                        for tg in bk['tags']:
                                name = tg['title']
                                if os.path.isfile(name.decode('utf-8')+'.json'):
                                        continue;
                                if name not in mark_tags:
                                        time.sleep(1)
                                        search_tag(name, 0)
                                        search_tag(name, 1)
        else:
                return


mark_tags = []

q = Queue.Queue()

logfile = 'tag_log.txt'

if not os.path.isfile(logfile):
	with open(logfile, 'w') as file:
		file.write(' ')

q.put('计算机')		
		
while not q.empty():
	time.sleep(1)
	tag = q.get()
	tmpurl = url[0] + urllib.quote(tag.encode('utf-8'))
	print tmpurl
	# if( depth > 5 ):
			# return
	mark_tags.append(tag)
	r = requests.get(tmpurl, headers=headers)
	with open(logfile, 'a') as output:
			output.write(tag+':'+str(r.status_code)+'\n')
	print tag.decode('utf-8')+':'+str(r.status_code)
	# print r.status_code
	if( r.status_code == 200 ):
			with open(tag.decode('utf-8')+'.json', 'w') as output:
					output.write(r.text)
			for bk in json.loads(r.text)['books']:
					for tg in bk['tags']:
							name = tg['title'].replace('\\','').replace('/','')
							if os.path.isfile(name.decode('utf-8')+'.json'):
									continue;
							if name not in mark_tags:
									q.put(name)
	else:
			time.sleep(300)


