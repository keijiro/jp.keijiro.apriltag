TARGET = AprilTag

SRCS = src/apriltag.c \
       src/apriltag_pose.c \
       src/apriltag_quad_thresh.c \
       src/tagStandard41h12.c \
       common/g2d.c \
       common/getopt.c \
       common/homography.c \
       common/image_u8.c \
       common/image_u8x3.c \
       common/image_u8x4.c \
       common/matd.c \
       common/pam.c \
       common/pjpeg-idct.c \
       common/pjpeg.c \
       common/pnm.c \
       common/string_util.c \
       common/svd22.c \
       common/time_util.c \
       common/unionfind.c \
       common/workerpool.c \
       common/zarray.c \
       common/zhash.c \
       common/zmaxheap.c \

OBJS = $(SRCS:.c=.o)

LIBS = -Wl,-Bstatic -lpthread

CCFLAGS = -O2 -Wall -I. -Iinclude
LDFLAGS = -shared

ifeq ($(PLATFORM), Windows)
    BIN_PREFIX = x86_64-w64-mingw32
    OUTPUT = $(TARGET).dll
else
    BIN_PREFIX =
    CCFLAGS += -fPIC
    LDFLAGS += -rdynamic -fPIC
    ifeq ($(PLATFORM), MacOS)
        OUTPUT = $(TARGET).bundle
	NOSTRIP = true
    else
        OUTPUT = lib$(TARGET).so
    endif
endif

CC = $(BIN_PREFIX)-gcc
STRIP = $(BIN_PREFIX)-strip

all: $(OUTPUT)

copy: $(OUTPUT)
ifndef NOSTRIP
	$(STRIP) $(OUTPUT)
endif
	cp -f $(OUTPUT) ../Assets/$(OUTPUT)

clean:
	rm -f $(OUTPUT) $(OBJS)

$(OUTPUT): $(OBJS)
	$(CC) $(LDFLAGS) -o $(OUTPUT) $(OBJS) $(LIBS)

.c.o:
	$(CC) $(CCFLAGS) -c -o $@ $<
