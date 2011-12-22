#!/bin/sh -e

PROGRAM="bin/rating_prediction"

echo "MyMediaLite online rating prediction test script"
echo "This will take about 3 minutes ..."

echo
echo "MovieLens 100k"
echo "--------------"

DATA_DIR=data/ml-100k

for method in GlobalAverage UserAverage ItemAverage Random Constant
do
       echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --online-evaluation
            $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --data-dir=$DATA_DIR --online-evaluation
done

for method in MatrixFactorization BiasedMatrixFactorization UserItemBaseline
do
       echo $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="num_iter=10" --data-dir=$DATA_DIR --online-evaluation
            $PROGRAM --training-file=u1.base --test-file=u1.test --recommender=$method --recommender-options="num_iter=10" --data-dir=$DATA_DIR --online-evaluation
done
