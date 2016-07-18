#include <windows.h>
#include <string>
#include "gtest/gtest.h"
#include <concrt.h>

// for testing SourceFileFinder

TEST(Crashing, LongRunning)
{
	Concurrency::wait(2000);
	EXPECT_EQ(1, 1);
}
